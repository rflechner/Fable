module Fable.AST.Fable.Util
open Fable
open Fable.AST

let attachRange (range: SourceLocation option) msg =
    match range with
    | Some range -> msg + " " + (string range)
    | None -> msg

type CallKind =
    | InstanceCall of callee: Expr * meth: string * args: Expr list
    | ImportCall of importPath: string * modName: string * meth: string option * isCons: bool * args: Expr list
    | CoreLibCall of modName: string * meth: string option * isCons: bool * args: Expr list
    | GlobalCall of modName: string * meth: string option * isCons: bool * args: Expr list

let makeLoop range loopKind = Loop (loopKind, range)
let makeCoreRef (com: ICompiler) modname = Value (ImportRef (modname, com.Options.coreLib))
let makeIdent name: Ident = {name=name; typ=UnknownType}
let makeIdentExpr name = makeIdent name |> IdentValue |> Value

let makeBinOp, makeUnOp, makeLogOp, makeEqOp =
    let makeOp range typ args op =
        Apply (Value op, args, ApplyMeth, typ, range)
    (fun range typ args op -> makeOp range typ args (BinaryOp op)),
    (fun range typ args op -> makeOp range typ args (UnaryOp op)),
    (fun range args op -> makeOp range (PrimitiveType Boolean) args (LogicalOp op)),
    (fun range args op -> makeOp range (PrimitiveType Boolean) args (BinaryOp op))

let rec makeSequential range statements =
    match statements with
    | [] -> Value Null
    | [expr] -> expr
    | first::rest ->
        match first, rest with
        | Value Null, _ -> makeSequential range rest
        | _, [Sequential (statements, _)] -> makeSequential range (first::statements)
        // Calls to System.Object..ctor in class constructors
        | ObjExpr ([],[],_,_), _ -> makeSequential range rest
        | _ -> Sequential (statements, range)

let makeConst (value: obj) =
    match value with
    | :? bool as x -> BoolConst x
    | :? string as x -> StringConst x
    | :? char as x -> StringConst (string x)
    // Integer types
    | :? int as x -> NumberConst (U2.Case1 x, Int32)
    | :? byte as x -> NumberConst (U2.Case1 (int x), UInt8)
    | :? sbyte as x -> NumberConst (U2.Case1 (int x), Int8)
    | :? int16 as x -> NumberConst (U2.Case1 (int x), Int16)
    | :? uint16 as x -> NumberConst (U2.Case1 (int x), UInt16)
    | :? uint32 as x -> NumberConst (U2.Case1 (int x), UInt32)
    // Float types
    | :? float as x -> NumberConst (U2.Case2 x, Float64)
    | :? int64 as x -> NumberConst (U2.Case2 (float x), Float64)
    | :? uint64 as x -> NumberConst (U2.Case2 (float x), Float64)
    | :? float32 as x -> NumberConst (U2.Case2 (float x), Float32)
    // TODO: Regex
    | :? unit | _ when value = null -> Null
    | _ -> failwithf "Unexpected literal %O" value
    |> Value

let makeFnType args =
    PrimitiveType (List.length args |> Function)

let makeGet range typ callee propExpr =
    Apply (callee, [propExpr], ApplyGet, typ, range)

let makeArray elementType arrExprs =
    let arrayKind =
        match elementType with
        | PrimitiveType (Number numberKind) -> TypedArray numberKind
        | _ -> DynamicArray
    ArrayConst(ArrayValues arrExprs, arrayKind) |> Value

let tryImported com name (decs: #seq<Decorator>) =
    decs |> Seq.tryPick (fun x ->
        match x.Name with
        | "Global" ->
            makeIdent name |> IdentValue |> Value |> Some
        | "Import" ->
            match x.Arguments with
            | [(:? string as memb);(:? string as path)] ->
                ImportRef(memb, path) |> Value |> Some
            | _ -> failwith "Import attributes must contain two string arguments"
        | _ -> None)

let makeTypeRef com (range: SourceLocation option) typ =
    match typ with
    | PrimitiveType _ ->
        "Cannot reference a primitive type"
        |> attachRange range |> failwith
    | UnknownType ->
        "Cannot reference unknown type. "
        + "If this a generic argument, try to make function inline."
        |> attachRange range |> failwith
    | DeclaredType ent ->
        match tryImported com ent.Name ent.Decorators with
        | Some expr -> expr
        | None -> Value (TypeRef ent)

let makeCall com range typ kind =
    let getCallee meth args owner =
        match meth with
        | None -> owner
        | Some meth ->
            let fnTyp = PrimitiveType (List.length args |> Function)
            Apply (owner, [makeConst meth], ApplyGet, fnTyp, None)
    let apply kind args callee =
        Apply(callee, args, kind, typ, range)
    let getKind isCons =
        if isCons then ApplyCons else ApplyMeth
    match kind with
    | InstanceCall (callee, meth, args) ->
        let fnTyp = PrimitiveType (List.length args |> Function)
        Apply (callee, [makeConst meth], ApplyGet, fnTyp, None)
        |> apply ApplyMeth args
    | ImportCall (importPath, modName, meth, isCons, args) ->
        Value (ImportRef (modName, importPath))
        |> getCallee meth args
        |> apply (getKind isCons) args
    | CoreLibCall (modName, meth, isCons, args) ->
        makeCoreRef com modName
        |> getCallee meth args
        |> apply (getKind isCons) args
    | GlobalCall (modName, meth, isCons, args) ->
        makeIdentExpr modName
        |> getCallee meth args
        |> apply (getKind isCons) args

let makeTypeTest com range (typ: Type) expr =
    let stringType, boolType =
        PrimitiveType String, PrimitiveType Boolean
    let checkType (primitiveType: string) expr =
        let typof = makeUnOp None stringType [expr] UnaryTypeof
        makeBinOp range boolType [typof; makeConst primitiveType] BinaryEqualStrict
    match typ with
    | PrimitiveType kind ->
        match kind with
        | String _ -> checkType "string" expr
        | Number _ -> checkType "number" expr
        | Boolean -> checkType "boolean" expr
        | Unit -> makeBinOp range boolType [expr; Value Null] BinaryEqual
        | Function _ -> checkType "function" expr
        // TODO: Regex and Array?
        | _ -> failwithf "Unsupported type test: %A" typ
    | DeclaredType typEnt ->
        match typEnt.Kind with
        | Interface ->
            CoreLibCall ("Util", Some "hasInterface", false, [expr; makeConst typEnt.FullName])
            |> makeCall com range boolType
        | _ ->
            makeBinOp range boolType [expr; makeTypeRef com range typ] BinaryInstanceOf
    | _ -> "Unsupported type test: " + typ.FullName
            |> attachRange range |> failwith

let makeUnionCons () =
    let emit = Emit "this.Case=arguments[0]; this.Fields = []; for (var i=1; i<arguments.length; i++) { this.Fields[(i-1)]=arguments[i]; }" |> Value
    let body = Apply (emit, [], ApplyMeth, PrimitiveType Unit, None)
    Member(Constructor, SourceLocation.Empty, [], body, [], true)
    |> MemberDeclaration

let makeExceptionCons () =
    let emit = Emit "for (var i=0; i<arguments.length; i++) { this['data'+i]=arguments[i]; }" |> Value
    let body = Apply (emit, [], ApplyMeth, PrimitiveType Unit, None)
    Member(Constructor, SourceLocation.Empty, [], body, [], true)
    |> MemberDeclaration

let makeRecordCons props =
    let sanitizeField x =
        if Naming.identForbiddenCharsRegex.IsMatch x
        then "['" + (x.Replace("'", "\\'")) + "']"
        else "." + x
    let args, body =
        props |> List.mapi (fun i _ -> sprintf "$arg%i" i |> makeIdent),
        props |> Seq.mapi (fun i x ->
            sprintf "this%s=$arg%i" (sanitizeField x) i) |> String.concat ";"
    let body = Apply (Value (Emit body), [], ApplyMeth, PrimitiveType Unit, None)
    Member(Constructor, SourceLocation.Empty, args, body, [], true, false, false)
    |> MemberDeclaration

let makeDelegate arity (expr: Expr) =
    let rec flattenLambda (arity: int option) accArgs = function
        | Value (Lambda (args, body)) when arity.IsNone || List.length accArgs < arity.Value ->
            flattenLambda arity (accArgs@args) body
        | _ as body ->
            Value (Lambda (accArgs, body))
    match expr, expr.Type with
    | Value (Lambda (args, body)), _ ->
        flattenLambda arity args body
    | _, PrimitiveType (Function a) ->
        let arity = defaultArg arity a
        if arity > 1 then
            let lambdaArgs =
                [for i=1 to arity do
                    yield {name=Naming.getUniqueVar(); typ=UnknownType}]
            let lambdaBody =
                (expr, lambdaArgs)
                ||> List.fold (fun callee arg ->
                    Apply (callee, [Value (IdentValue arg)],
                        ApplyMeth, UnknownType, expr.Range))
            Lambda (lambdaArgs, lambdaBody) |> Value
        else
            expr // Do nothing
    | _ -> expr

// Check if we're applying against a F# let binding
let makeApply range typ callee exprs =
    let lasti = (List.length exprs) - 1
    ((0, callee), exprs)
    ||> List.fold (fun (i, callee) expr ->
        let typ = if i = lasti then typ else PrimitiveType (Function <|i+1)
        let callee =
            match callee with
            | Sequential _ ->
                // F# let binding: Surround with a lambda
                Apply (Lambda ([], callee) |> Value, [], ApplyMeth, typ, range)
            | _ -> callee
        i, Apply (callee, [expr], ApplyMeth, typ, range))
    |> snd

let makeJsObject range (props: (string * Expr) list) =
    let members = props |> List.map (fun (name, body) ->
        Member(Getter (name, true), range, [], body))
    ObjExpr(members, [], None, Some range)
    
let getTypedArrayName (com: ICompiler) numberKind =
    match numberKind with
    | Int8 -> "Int8Array"
    | UInt8 -> if com.Options.clamp then "Uint8ClampedArray" else "Uint8Array"
    | Int16 -> "Int16Array"
    | UInt16 -> "Uint16Array"
    | Int32 -> "Int32Array"
    | UInt32 -> "Uint32Array"
    | Float32 -> "Float32Array"
    | Float64 -> "Float64Array"
