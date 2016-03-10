module Fable.FSharp2Fable.Compiler

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.SourceCodeServices

open Fable
open Fable.AST
open Fable.AST.Fable.Util

open Patterns
open Types
open Identifiers
open Util

// Special values like seq, async, String.Empty...
let private (|SpecialValue|_|) com ctx = function
    | BasicPatterns.Value v ->
        match v.FullName with
        | "Microsoft.FSharp.Core.Operators.seq" ->
            makeCoreRef com "Seq" |> Some
        | "Microsoft.FSharp.Core.ExtraTopLevelOperators.async" ->
            makeCoreRef com "Async" |> Some
        | _ -> None
    | BasicPatterns.ILFieldGet (None, typ, fieldName) as fsExpr when typ.HasTypeDefinition ->
        match typ.TypeDefinition.FullName, fieldName with
        | "System.String", "Empty" -> Some (makeConst "")
        | "System.TimeSpan", "Zero" ->
            Fable.Wrapped(makeConst 0, makeType com ctx fsExpr.Type) |> Some
        | "System.DateTime", "MaxValue"
        | "System.DateTime", "MinValue" ->
            CoreLibCall("Date", Some (Naming.lowerFirst fieldName), false, [])
            |> makeCall com (makeRangeFrom fsExpr) (makeType com ctx fsExpr.Type) |> Some 
        | _ -> None
    | _ -> None

let rec private transformExpr (com: IFableCompiler) ctx fsExpr =
    match fsExpr with
    (** ## Custom patterns *)
    | SpecialValue com ctx replacement ->
        replacement
    
    // TODO: Detect if it's ResizeArray and compile as FastIntegerForLoop?
    | ForOf (BindIdent com ctx (newContext, ident), Transform com ctx value, body) ->
        Fable.ForOf (ident, value, transformExpr com newContext body)
        |> makeLoop (makeRangeFrom fsExpr)
        
    | ErasableLambda (meth, typArgs, methTypArgs, methArgs) ->
        makeCallFrom com ctx fsExpr meth (typArgs, methTypArgs)
            None (List.map (com.Transform ctx) methArgs)

    // Pipe must come after ErasableLambda
    | Pipe (Transform com ctx callee, args) ->
        let typ, range = makeType com ctx fsExpr.Type, makeRangeFrom fsExpr
        makeApply range typ callee (List.map (transformExpr com ctx) args)
        
    | Composition (meth1, typArgs1, methTypArgs1, args1, meth2, typArgs2, methTypArgs2, args2) ->
        let lambdaArg = makeIdent "$arg"
        let expr1 =
            (List.map (com.Transform ctx) args1)@[Fable.Value (Fable.IdentValue lambdaArg)]
            |> makeCallFrom com ctx fsExpr meth1 (typArgs1, methTypArgs1) None
        let expr2 =
            (List.map (com.Transform ctx) args2)@[expr1]
            |> makeCallFrom com ctx fsExpr meth2 (typArgs2, methTypArgs2) None
        Fable.Lambda([lambdaArg], expr2) |> Fable.Value
            
    (** ## Erased *)
    | BasicPatterns.Coerce(_targetType, Transform com ctx inpExpr) -> inpExpr
    // TypeLambda is a local generic lambda
    // e.g, member x.Test() = let typeLambda x = x in typeLambda 1, typeLambda "A"
    | BasicPatterns.TypeLambda (_genArgs, Transform com ctx lambda) -> lambda

    (** ## Flow control *)
    | BasicPatterns.FastIntegerForLoop(Transform com ctx start, Transform com ctx limit, body, isUp) ->
        match body with
        | BasicPatterns.Lambda (BindIdent com ctx (newContext, ident), body) ->
            Fable.For (ident, start, limit, com.Transform newContext body, isUp)
            |> makeLoop (makeRangeFrom fsExpr)
        | _ -> failwithf "Unexpected loop in %A: %A" fsExpr.Range fsExpr

    | BasicPatterns.WhileLoop(Transform com ctx guardExpr, Transform com ctx bodyExpr) ->
        Fable.While (guardExpr, bodyExpr)
        |> makeLoop (makeRangeFrom fsExpr)

    (** Values *)

    // Arrays with small data (ushort, byte) won't fit the NewArray pattern
    // as they would require too much memory
    | BasicPatterns.Const(:? System.Collections.IEnumerable as arr, typ)
        when typ.HasTypeDefinition && typ.TypeDefinition.IsArrayType ->
        let mutable argExprs = []
        let enumerator = arr.GetEnumerator()
        while enumerator.MoveNext() do
            argExprs <- (makeConst enumerator.Current)::argExprs
        makeArray (makeType com ctx typ) (argExprs |> List.rev)

    | BasicPatterns.Const(value, FableType com ctx typ) ->
        let e = makeConst value
        if e.Type = typ then e
        // Enumerations are compiled as const but they have a different type
        else Fable.Wrapped (e, typ)

    | BasicPatterns.BaseValue typ ->
        Fable.Super |> Fable.Value 

    | BasicPatterns.ThisValue typ ->
        Fable.This |> Fable.Value 

    | BasicPatterns.Value thisVar when thisVar.IsMemberThisValue ->
        Fable.This |> Fable.Value 

    | BasicPatterns.Value v ->
        if not v.IsModuleValueOrMember
        then getBoundExpr com ctx v
        elif v.IsMemberThisValue
        then Fable.This |> Fable.Value
        // External entities contain functions that will be replaced,
        // when they appear as a stand alone values, they must be wrapped in a lambda
        elif isExternalEntity com v.EnclosingEntity
        then wrapInLambda com ctx fsExpr v
        else
            v.Attributes
            |> Seq.choose (makeDecorator com)
            |> tryImported com v.DisplayName
            |> function
                | Some expr -> expr
                | None ->
                    let typeRef =
                        makeTypeFromDef com v.EnclosingEntity
                        |> makeTypeRef com (makeRange fsExpr.Range)
                    makeGetFrom com ctx fsExpr typeRef (makeConst v.DisplayName)

    | BasicPatterns.DefaultValue (FableType com ctx typ) ->
        let valueKind =
            match typ with
            | Fable.PrimitiveType Fable.Boolean -> Fable.BoolConst false
            | Fable.PrimitiveType (Fable.Number kind) -> Fable.NumberConst (U2.Case1 0, kind)
            | _ -> Fable.Null
        Fable.Value valueKind

    (** ## Assignments *)
    | BasicPatterns.Let((var, Transform com ctx value), body) ->
        let ctx, ident = bindIdentFrom com ctx var
        let body = transformExpr com ctx body
        let assignment = Fable.VarDeclaration (ident, value, var.IsMutable) 
        makeSequential (makeRangeFrom fsExpr) [assignment; body]

    | BasicPatterns.LetRec(recBindings, body) ->
        let ctx, idents =
            (recBindings, (ctx, [])) ||> List.foldBack (fun (var,_) (ctx, idents) ->
                let (BindIdent com ctx (newContext, ident)) = var
                (newContext, ident::idents))
        let assignments =
            recBindings
            |> List.map2 (fun ident (var, Transform com ctx binding) ->
                Fable.VarDeclaration (ident, binding, var.IsMutable)) idents
        assignments @ [transformExpr com ctx body] 
        |> makeSequential (makeRangeFrom fsExpr)

    (** ## Applications *)
    | BasicPatterns.TraitCall (_sourceTypes, traitName, _typeArgs, _typeInstantiation, argExprs) ->
        // printfn "TraitCall detected in %A: %A" fsExpr.Range fsExpr // TODO: Check
        let range = makeRangeFrom fsExpr
        let callee, args = transformExpr com ctx argExprs.Head, List.map (transformExpr com ctx) argExprs.Tail
        let callee = makeGet range (Fable.PrimitiveType (Fable.Function argExprs.Length)) callee (makeConst traitName)
        Fable.Apply (callee, args, Fable.ApplyMeth, makeType com ctx fsExpr.Type, range)

    | BasicPatterns.Call(callee, meth, typArgs, methTypArgs, args) ->
        let methOwnerName (meth: FSharpMemberOrFunctionOrValue) =
            sanitizeEntityName meth.EnclosingEntity
        let callee, args = Option.map (com.Transform ctx) callee, List.map (com.Transform ctx) args
        match ctx.owner with
        | Some (EntityKind(Fable.Class(Some (baseFullName,_))) as ent) when (methOwnerName meth) = baseFullName ->
            if not meth.IsImplicitConstructor then
                failwithf "Inheritance is only possible with base class implicit constructor: %s" ent.FullName
            let typ, range = makeType com ctx fsExpr.Type, makeRangeFrom fsExpr
            Fable.Apply(Fable.Value Fable.Super, args, Fable.ApplyMeth, typ, range)
        | _ -> makeCallFrom com ctx fsExpr meth (typArgs, methTypArgs) callee args

    | BasicPatterns.Application(Transform com ctx callee, _typeArgs, args) ->
        let typ, range = makeType com ctx fsExpr.Type, makeRangeFrom fsExpr
        makeApply range typ callee (List.map (transformExpr com ctx) args)
        
    | BasicPatterns.IfThenElse (Transform com ctx guardExpr, Transform com ctx thenExpr, Transform com ctx elseExpr) ->
        Fable.IfThenElse (guardExpr, thenExpr, elseExpr, makeRangeFrom fsExpr)

    | BasicPatterns.TryFinally (BasicPatterns.TryWith(body, _, _, catchVar, catchBody),finalBody) ->
        makeTryCatch com ctx fsExpr body (Some (catchVar, catchBody)) (Some finalBody)

    | BasicPatterns.TryFinally (body, finalBody) ->
        makeTryCatch com ctx fsExpr body None (Some finalBody)

    | BasicPatterns.TryWith (body, _, _, catchVar, catchBody) ->
        makeTryCatch com ctx fsExpr body (Some (catchVar, catchBody)) None

    | BasicPatterns.Sequential (Transform com ctx first, Transform com ctx second) ->
        makeSequential (makeRangeFrom fsExpr) [first; second]

    (** ## Lambdas *)
    | BasicPatterns.Lambda (var, body) ->
        let ctx, args = makeLambdaArgs com ctx [var]
        Fable.Lambda (args, transformExpr com ctx body) |> Fable.Value

    | BasicPatterns.NewDelegate(_delegateType, Transform com ctx delegateBodyExpr) ->
        makeDelegate delegateBodyExpr

    (** ## Getters and Setters *)
    // TODO: Check if it's FSharpException?
    // TODO: Change name of automatically generated fields
    | BasicPatterns.FSharpFieldGet (callee, FableType com ctx calleeType, FieldName fieldName) ->
        let callee =
            match callee with
            | Some (Transform com ctx callee) -> callee
            | None -> makeTypeRef com (makeRange fsExpr.Range) calleeType
        makeGetFrom com ctx fsExpr callee (makeConst fieldName)

    | BasicPatterns.TupleGet (_tupleType, tupleElemIndex, Transform com ctx tupleExpr) ->
        makeGetFrom com ctx fsExpr tupleExpr (makeConst tupleElemIndex)

    // Single field: data; Multiple fields: data[0], data[1]...
    | BasicPatterns.UnionCaseGet (Transform com ctx unionExpr, FableType com ctx unionType, unionCase, FieldName fieldName) ->
        match unionType with
        | ErasedUnion | OptionUnion -> unionExpr
        | ListUnion ->
            makeGet (makeRangeFrom fsExpr) (makeType com ctx fsExpr.Type)
                    unionExpr (Naming.lowerFirst fieldName |> makeConst)
        | OtherType ->
            let typ, range = makeType com ctx fsExpr.Type, makeRangeFrom fsExpr
            let dataProp = makeGet range typ unionExpr (makeConst "data")
            match Naming.getFieldIndex fieldName with
            | 0 -> dataProp
            | i -> makeGet range typ dataProp (makeConst (i-1))

    | BasicPatterns.ILFieldSet (callee, typ, fieldName, value) ->
        failwithf "Found unsupported ILField reference in %A: %A" fsExpr.Range fsExpr

    // TODO: Change name of automatically generated fields
    | BasicPatterns.FSharpFieldSet (callee, FableType com ctx calleeType, FieldName fieldName, Transform com ctx value) ->
        let callee =
            match callee with
            | Some (Transform com ctx callee) -> callee
            | None -> makeTypeRef com (makeRange fsExpr.Range) calleeType
        Fable.Set (callee, Some (makeConst fieldName), value, makeRangeFrom fsExpr)

    | BasicPatterns.UnionCaseTag (Transform com ctx unionExpr, _unionType) ->
        makeGetFrom com ctx fsExpr unionExpr (makeConst "tag")

    | BasicPatterns.UnionCaseSet (Transform com ctx unionExpr, _type, _case, FieldName caseField, Transform com ctx valueExpr) ->
        failwith "Unexpected UnionCaseSet"

    | BasicPatterns.ValueSet (valToSet, Transform com ctx valueExpr) ->
        let valToSet = getBoundExpr com ctx valToSet
        Fable.Set (valToSet, None, valueExpr, makeRangeFrom fsExpr)

    (** Instantiation *)
    | BasicPatterns.NewArray(FableType com ctx typ, argExprs) ->
        makeArray typ (argExprs |> List.map (transformExpr com ctx))

    | BasicPatterns.NewTuple(_, argExprs) ->
        (argExprs |> List.map (transformExpr com ctx) |> Fable.ArrayValues, Fable.Tuple)
        |> Fable.ArrayConst |> Fable.Value

    | BasicPatterns.ObjectExpr(objType, baseCallExpr, overrides, otherOverrides) ->
        let lowerFirstKnownInterfaces typName name =
            if Naming.knownInterfaces.Contains typName
            then Naming.lowerFirst name
            else name
        match baseCallExpr with
        | BasicPatterns.Call(None, meth, [], [], []) when meth.EnclosingEntity.FullName = "System.Object" ->
            let members =
                (objType, overrides)::otherOverrides
                |> List.map (fun (typ, overrides) ->
                    let typName = sanitizeEntityName typ.TypeDefinition
                    overrides |> List.map (fun over ->
                        let args, range = over.CurriedParameterGroups, makeRange fsExpr.Range
                        let ctx, args' = getMethodArgs com ctx true args
                        let kind =
                            let name =
                                over.Signature.Name
                                |> Naming.removeParens
                                |> Naming.removeGetSetPrefix
                                |> lowerFirstKnownInterfaces typName
                            match over.Signature.Name with
                            | Naming.StartsWith "get_" _ -> Fable.Getter (name, false)
                            | Naming.StartsWith "set_" _ -> Fable.Setter name
                            | _ -> Fable.Method name
                        Fable.Member(kind, range, args', transformExpr com ctx over.Body,
                            [], true, false, hasRestParams args)))
                |> List.concat
            let interfaces =
                objType::(otherOverrides |> List.map fst)
                |> List.map (fun x -> sanitizeEntityName x.TypeDefinition)
                |> List.distinct
            Fable.ObjExpr (members, interfaces, makeRangeFrom fsExpr)
        | _ -> failwithf "Object expression from classes are not supported: %A" fsExpr.Range

    // TODO: Check for erased constructors with property assignment (Call + Sequential)
    | BasicPatterns.NewObject(meth, typArgs, args) ->
        makeCallFrom com ctx fsExpr meth (typArgs, []) None (List.map (com.Transform ctx) args)

    | BasicPatterns.NewRecord(NonAbbreviatedType fsType, argExprs) ->
        let recordType, range = makeType com ctx fsType, makeRange fsExpr.Range
        let argExprs = argExprs |> List.map (transformExpr com ctx)
        if isExternalEntity com fsType.TypeDefinition
        then replace com ctx fsExpr (recordType.FullName) ".ctor" ([],[],[]) (None,argExprs)
        else Fable.Apply (makeTypeRef com range recordType, argExprs, Fable.ApplyCons,
                        makeType com ctx fsExpr.Type, Some range)

    | BasicPatterns.NewUnionCase(NonAbbreviatedType fsType, unionCase, argExprs) ->
        let unionType, range = makeType com ctx fsType, makeRange fsExpr.Range
        match unionType with
        | ErasedUnion | OptionUnion ->
            match List.map (transformExpr com ctx) argExprs with
            | [] -> Fable.Value Fable.Null 
            | [expr] -> expr
            | _ -> failwithf "Erased Union Cases must have one single field: %A" unionType
        | ListUnion ->
            let buildArgs args =
                let args = args |> List.rev |> (List.map (transformExpr com ctx))
                Fable.Value (Fable.ArrayConst (Fable.ArrayValues args, Fable.DynamicArray))
            let rec ofArray accArgs = function
                | [] ->
                    CoreLibCall("List", Some "ofArray", false, [buildArgs accArgs])
                | arg::[BasicPatterns.NewUnionCase(_, _, rest)] ->
                    ofArray (arg::accArgs) rest
                | arg::[Transform com ctx list2] ->
                    CoreLibCall("List", Some "ofArray", false, (buildArgs (arg::accArgs))::[list2])
                | _ ->
                    failwithf "Unexpected List constructor %A at %A" fsExpr fsExpr.Range
            match argExprs with
            | [] -> CoreLibCall("List", None, true, [])
            | _ -> ofArray [] argExprs
            |> makeCall com (Some range) unionType
        | OtherType ->
            let argExprs =
                // Include Tag name in args
                let tag = makeConst unionCase.Name
                match List.map (transformExpr com ctx) argExprs with
                | [] -> [tag]
                | [arg] -> [tag;arg]
                // If there's more than one data field, make a tuple
                | args -> [tag; Fable.ArrayConst(Fable.ArrayValues args, Fable.Tuple) |> Fable.Value]
            if isExternalEntity com fsType.TypeDefinition
            then replace com ctx fsExpr (unionType.FullName) ".ctor" ([],[],[]) (None,argExprs)
            else Fable.Apply (makeTypeRef com range unionType, argExprs, Fable.ApplyCons,
                            makeType com ctx fsExpr.Type, Some range)

    (** ## Type test *)
    | BasicPatterns.TypeTest (FableType com ctx typ as fsTyp, Transform com ctx expr) ->
        makeTypeTest com (makeRangeFrom fsExpr) typ expr 

    | BasicPatterns.UnionCaseTest (Transform com ctx unionExpr, FableType com ctx unionType, unionCase) ->
        let boolType = Fable.PrimitiveType Fable.Boolean
        match unionType with
        | ErasedUnion ->
            if unionCase.UnionCaseFields.Count <> 1 then
                failwithf "Erased Union Cases must have one single field: %A" unionType
            else
                let typ = makeType com ctx unionCase.UnionCaseFields.[0].FieldType
                makeTypeTest com (makeRangeFrom fsExpr) typ unionExpr
        | OptionUnion ->
            let opKind = if unionCase.Name = "None" then BinaryEqual else BinaryUnequal
            makeBinOp (makeRangeFrom fsExpr) boolType [unionExpr; Fable.Value Fable.Null] opKind 
        | ListUnion ->
            let opKind = if unionCase.CompiledName = "Empty" then BinaryEqual else BinaryUnequal
            let expr = makeGet None Fable.UnknownType unionExpr (makeConst "tail")
            makeBinOp (makeRangeFrom fsExpr) boolType [expr; Fable.Value Fable.Null] opKind 
        | OtherType ->
            let left = makeGet None (Fable.PrimitiveType Fable.String) unionExpr (makeConst "tag")
            let right = makeConst unionCase.Name
            makeBinOp (makeRangeFrom fsExpr) boolType [left; right] BinaryEqualStrict

    (** Pattern Matching *)
    | BasicPatterns.DecisionTree(decisionExpr, decisionTargets) ->
        let rec getTargetRefsCount map = function
            | BasicPatterns.IfThenElse (_, thenExpr, elseExpr)
            | BasicPatterns.Let(_, BasicPatterns.IfThenElse (_, thenExpr, elseExpr)) ->
                let map = getTargetRefsCount map thenExpr
                getTargetRefsCount map elseExpr
            | BasicPatterns.DecisionTreeSuccess (idx, _) ->
                match (Map.tryFind idx map) with
                | Some refCount -> Map.remove idx map |> Map.add idx (refCount + 1)
                | None -> Map.add idx 1 map
            | _ as e ->
                failwithf "Unexpected DecisionTree branch in %A: %A" e.Range e
        let targetRefsCount = getTargetRefsCount (Map.empty<int,int>) decisionExpr
        // Convert targets referred more than once into functions
        // and just pass the F# implementation for the others
        let ctx, assignments =
            targetRefsCount
            |> Map.filter (fun k v -> v > 1)
            |> Map.fold (fun (ctx, acc) k v ->
                let targetVars, targetExpr = decisionTargets.[k]
                let targetVars, targetCtx =
                    (targetVars, ([], ctx)) ||> List.foldBack (fun var (vars, ctx) ->
                        let ctx, var = bindIdentFrom com ctx var
                        var::vars, ctx)
                let lambda =
                    Fable.Lambda (targetVars, com.Transform targetCtx targetExpr)
                    |> Fable.Value
                let ctx, ident = bindIdent ctx lambda.Type (sprintf "target%i" k)
                ctx, Map.add k (ident, lambda) acc) (ctx, Map.empty<_,_>)
        let decisionTargets =
            targetRefsCount |> Map.map (fun k v ->
                match v with
                | 1 -> TargetImpl decisionTargets.[k]
                | _ -> TargetRef (fst assignments.[k]))
        let ctx = { ctx with decisionTargets = decisionTargets }
        if assignments.Count = 0 then
            transformExpr com ctx decisionExpr
        else
            let assignments =
                assignments
                |> Seq.map (fun pair -> pair.Value)
                |> Seq.map (fun (ident, lambda) ->
                    Fable.VarDeclaration (ident, lambda, false))
                |> Seq.toList
            Fable.Sequential (assignments @ [transformExpr com ctx decisionExpr], makeRangeFrom fsExpr)

    | BasicPatterns.DecisionTreeSuccess (decIndex, decBindings) ->
        match Map.tryFind decIndex ctx.decisionTargets with
        | None -> failwith "Missing decision target"
        // If we get a reference to a function, call it
        | Some (TargetRef targetRef) ->
            Fable.Apply (Fable.IdentValue targetRef |> Fable.Value,
                (decBindings |> List.map (transformExpr com ctx)),
                Fable.ApplyMeth, makeType com ctx fsExpr.Type, makeRangeFrom fsExpr)
        // If we get an implementation without bindings, just transform it
        | Some (TargetImpl ([], Transform com ctx decBody)) -> decBody
        // If we have bindings, create the assignments
        | Some (TargetImpl (decVars, decBody)) ->
            let newContext, assignments =
                List.foldBack2 (fun var (Transform com ctx binding) (accContext, accAssignments) ->
                    let (BindIdent com accContext (newContext, ident)) = var
                    let assignment = Fable.VarDeclaration (ident, binding, var.IsMutable)
                    newContext, (assignment::accAssignments)) decVars decBindings (ctx, [])
            assignments @ [transformExpr com newContext decBody]
            |> makeSequential (makeRangeFrom fsExpr)

    (** Not implemented *)
    | BasicPatterns.ILAsm _
    | BasicPatterns.ILFieldGet _
    | BasicPatterns.Quote _ // (quotedExpr)
    | BasicPatterns.AddressOf _ // (lvalueExpr)
    | BasicPatterns.AddressSet _ // (lvalueExpr, rvalueExpr)
    | _ -> failwithf "Cannot compile expression in %A: %A" fsExpr.Range fsExpr

// The F# compiler considers class methods as children of the enclosing module.
// We use this type to correct that, see type DeclInfo below.
type private EntChild =
    | Compiled of Fable.Entity * SourceLocation
    | Ignored of fullName: string
    static member matchesFullName child str =
        match child with
        | Some (Compiled (ent, _)) -> ent.FullName = str
        | Some (Ignored fullName) -> fullName = str
        | None -> false

type private DeclInfo(init: Fable.Declaration list) =
    let ignoredAtts = set ["Erase"; "Import"; "Global"; "Emit"]
    let mutable child: EntChild option = None
    let decls = ResizeArray<Fable.Declaration>(init)
    let childDecls = ResizeArray<Fable.Declaration>()
    let hasIgnoredAtt atts =
        atts |> tryFindAtt (ignoredAtts.Contains) |> Option.isSome
    member self.CurrentEntity =
       match child with
       | Some (Compiled (ent, _)) -> Some ent
       | _ -> None
    /// Interface, inherits from System.Attribute, has "Erase" decorator...
    member self.IsIgnoredEntity (ent: FSharpEntity) =
        if ent.IsInterface || (hasIgnoredAtt ent.Attributes)
        then true
        else match ent.BaseType with
             | Some (NonAbbreviatedType t) when t.HasTypeDefinition ->
                 match t.TypeDefinition.TryFullName with
                 | Some "System.Attribute" -> true
                 | _ -> false
             | _ -> false
    /// Is compiler generated (CompareTo...) or belongs to ignored entity?
    /// (remember F# compiler puts class methods in enclosing modules)
    member self.IsIgnoredMethod (meth: FSharpMemberOrFunctionOrValue) =
        if (meth.IsCompilerGenerated && Naming.ignoredCompilerGenerated.Contains meth.DisplayName)
            || (hasIgnoredAtt meth.Attributes)
        then true
        else match child with
             | Some (Ignored fullName) ->
                 (sanitizeEntityName meth.EnclosingEntity) = fullName
             | _ -> false
    member self.AddMethod (methDecl: Fable.Declaration, parentName: string) =
        EntChild.matchesFullName child parentName
        |> function true -> childDecls.Add methDecl
                  | false -> self.ClearChild (); decls.Add methDecl
    member self.AddInitAction (actionDecl: Fable.Declaration) =
        self.ClearChild ()
        decls.Add actionDecl
    member self.ClearChild () =
        match child with
        | Some (Compiled (child, range)) ->
            Fable.EntityDeclaration (child, List.ofSeq childDecls, range)
            |> decls.Add
        | _ -> ()
        child <- None
        childDecls.Clear ()
    member self.AddChild (newChild, newChildRange, newChildDecls: Fable.Declaration list) =
        self.ClearChild ()
        let newChildRange =
            match newChildDecls with
            | [] -> newChildRange
            | decls -> newChildRange + (List.last decls).Range
        child <- Some (Compiled (newChild, newChildRange))
        childDecls.AddRange newChildDecls
    member self.AddIgnored (ent: FSharpEntity) =
        self.ClearChild ()
        child <- Some (Ignored (sanitizeEntityName ent))
    member self.GetDeclarations () =
        self.ClearChild ()
        List.ofSeq decls        
    
let private transformMemberDecl (com: IFableCompiler) ctx (declInfo: DeclInfo)
    (meth: FSharpMemberOrFunctionOrValue) (args: FSharpMemberOrFunctionOrValue list list) (body: FSharpExpr) =
    match meth with
    | meth when declInfo.IsIgnoredMethod meth -> ()
    | meth when isInline meth ->
        let args = args |> Seq.collect id |> Seq.map (fun x -> x.DisplayName)
        com.AddInlineExpr meth.FullName (List.ofSeq args, body)
    | _ ->
        let memberKind =
            let name = sanitizeMethodName com meth
            // TODO: Another way to check module values?
            if meth.EnclosingEntity.IsFSharpModule
            then match meth.XmlDocSig.[0] with
                 | 'P' -> Fable.Getter (name, true)
                 | _ -> Fable.Method name
            else getMemberKind name meth
        let ctx, args' =
            getMethodArgs com ctx meth.IsInstanceMember args
        let body =
            let ctx =
                if meth.IsImplicitConstructor
                then { ctx with owner=declInfo.CurrentEntity }
                else ctx
            transformExpr com ctx body
        let entMember = 
            Fable.Member(memberKind,
                makeRange meth.DeclarationLocation, args', body,
                meth.Attributes |> Seq.choose (makeDecorator com) |> Seq.toList,
                meth.Accessibility.IsPublic, not meth.IsInstanceMember, hasRestParams args)
            |> Fable.MemberDeclaration
        declInfo.AddMethod (entMember, sanitizeEntityName meth.EnclosingEntity)
    declInfo
   
let rec private transformEntityDecl
    (com: IFableCompiler) ctx (declInfo: DeclInfo) (ent: FSharpEntity) subDecls =
    if declInfo.IsIgnoredEntity ent
    then declInfo.AddIgnored ent; declInfo
    else
        let entRange = makeRange ent.DeclarationLocation
        // Unions and Records don't have a constructor, generate it
        let init =
            if ent.IsFSharpUnion
            then [entRange.Collapse() |> makeUnionCons]
            elif ent.IsFSharpExceptionDeclaration
            then [entRange.Collapse() |> makeExceptionCons]
            elif ent.IsFSharpRecord
            then ent.FSharpFields
                 |> Seq.map (fun x -> x.DisplayName) |> Seq.toList
                 |> makeRecordCons (entRange.Collapse())
                 |> List.singleton
            else []
        let childDecls = transformDeclarations com ctx init subDecls
        declInfo.AddChild (com.GetEntity ent, entRange, childDecls)
        declInfo

and private transformDeclarations (com: IFableCompiler) ctx init decls =
    let declInfo =
        decls |> List.fold (fun (declInfo: DeclInfo) decl ->
            match decl with
            | FSharpImplementationFileDeclaration.Entity (e, sub) ->
                if e.IsFSharpAbbreviation
                then declInfo
                else transformEntityDecl com ctx declInfo e sub
            | FSharpImplementationFileDeclaration.MemberOrFunctionOrValue (meth, args, body) ->
                transformMemberDecl com ctx declInfo meth args body
            | FSharpImplementationFileDeclaration.InitAction (Transform com ctx e as fe) ->
                declInfo.AddInitAction (Fable.ActionDeclaration (e, makeRange fe.Range)); declInfo
        ) (DeclInfo init)
    declInfo.GetDeclarations ()
        
let transformFiles (com: ICompiler) (fileMask: string option) (fsProj: FSharpCheckProjectResults) =
    let rec getRootDecls rootEnt = function
        | [FSharpImplementationFileDeclaration.Entity (e, subDecls)]
            when e.IsNamespace || e.IsFSharpModule ->
            getRootDecls (Some e) subDecls
        | _ as decls -> rootEnt, decls
    let entities, inlineExprs =
        System.Collections.Concurrent.ConcurrentDictionary<string, Fable.Entity>(),
        System.Collections.Concurrent.ConcurrentDictionary<string, string list * FSharpExpr>()
    let fileNames =
        fsProj.AssemblyContents.ImplementationFiles
        |> Seq.map (fun x -> x.FileName)
        |> Seq.where (fun file -> (System.IO.Path.GetFileName file) <> "Fable.Core.fs")
        |> Set.ofSeq
    let replacePlugins =
        com.Plugins |> List.choose (function
            | :? IReplacePlugin as plugin -> Some plugin
            | _ -> None)
    let com =
        { new IFableCompiler with
            member fcom.Transform ctx fsExpr =
                transformExpr fcom ctx fsExpr
            member fcom.GetInternalFile tdef =
                // In F# scripts the DeclarationLocation of referenced libraries
                // becomes the .fsx file, so check first if the entity belongs
                // to an assembly already compiled (external to the project)
                match tdef.Assembly.FileName with
                | Some _ -> None
                | None ->
                    let file = tdef.DeclarationLocation.FileName
                    if Set.contains file fileNames then Some file else None
            member fcom.GetEntity tdef =
                entities.GetOrAdd (tdef.FullName, fun _ -> makeEntity fcom tdef)
            member fcom.TryGetInlineExpr fullName =
                let success, expr = inlineExprs.TryGetValue fullName
                if success then Some expr else None
            member fcom.AddInlineExpr fullName inlineExpr =
                inlineExprs.TryAdd(fullName, inlineExpr)
                |> ignore
            member fcom.ReplacePlugins =
                replacePlugins
        interface ICompiler with
            member __.Options = com.Options
            member __.Plugins = com.Plugins }
    fsProj.AssemblyContents.ImplementationFiles
    |> List.where (fun file ->
        // Fable.Import files are not considered external (the methods are not replaced)
        // but we ignore them for compilation
        ((System.IO.Path.GetFileName file.FileName).StartsWith("Fable.Import") |> not)
        && fileNames.Contains file.FileName)
    |> List.map (fun file ->
        try
            let rootEnt, rootDecls =
                let rootEnt, rootDecls =
                    let rootEnt, rootDecls = getRootDecls None file.Declarations
                    match fileMask with
                    | Some mask when file.FileName <> mask -> rootEnt, []
                    | _ -> rootEnt, transformDeclarations com Context.Empty [] rootDecls
                match rootEnt with
                | Some rootEnt -> makeEntity com rootEnt, rootDecls
                | None -> Fable.Entity.CreateRootModule file.FileName, rootDecls
            Fable.File(file.FileName, rootEnt, rootDecls)
        with
        | ex -> failwithf "%s (%s)" ex.Message file.FileName)
