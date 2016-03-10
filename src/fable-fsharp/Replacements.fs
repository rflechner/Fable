module Fable.Replacements
open Fable
open Fable.AST
open Fable.AST.Fable.Util

module Util =
    let [<Literal>] system = "System."
    let [<Literal>] fsharp = "Microsoft.FSharp."
    let [<Literal>] genericCollections = "System.Collections.Generic."

    let inline (=>) first second = first, second

    let (|DicContains|_|) (dic: System.Collections.Generic.IDictionary<'k,'v>) key =
        let success, value = dic.TryGetValue key
        if success then Some value else None

    let (|SetContains|_|) set item =
        if Set.contains item set then Some item else None
        
    let (|KnownInterfaces|_|) fullName =
        if Naming.knownInterfaces.Contains fullName then Some fullName else None
        
    let (|CoreMeth|_|) com coreMod meth expr =
        match expr with
        | Fable.Apply(Fable.Value(Fable.ImportRef(import, false , Some coreMod')),
                      [Fable.Value(Fable.StringConst meth')], Fable.ApplyGet,_,_)
            when import = (Naming.getCoreLibPath com) && coreMod = coreMod' && meth = meth' -> Some expr
        | _ -> None

    let (|Null|_|) = function
        | Fable.Value Fable.Null -> Some null
        | _ -> None

    let (|Type|) (expr: Fable.Expr) = expr.Type
        
    let (|FullName|_|) (typ: Fable.Type) =
        match typ with
        | Fable.DeclaredType ent -> Some ent.FullName
        | _ -> None
        
    let (|KeyValue|_|) (key: string) (value: string) (s: string) =
        if s = key then Some value else None

    let (|OneArg|_|) (callee: Fable.Expr option, args: Fable.Expr list) =
        match callee, args with None, arg::_ -> Some arg | _ -> None

    let (|TwoArgs|_|) (callee: Fable.Expr option, args: Fable.Expr list) =
        match callee, args with None, left::right::_ -> Some (left, right) | _ -> None

    let (|ThreeArgs|_|) (callee: Fable.Expr option, args: Fable.Expr list) =
        match callee, args with None, arg1::arg2::arg3::_ -> Some (arg1, arg2, arg3) | _ -> None

    // The core lib expects non-curried lambdas
    let deleg = List.mapi (fun i x ->
        if i=0 then (makeDelegate x) else x)

    let instanceArgs (callee: Fable.Expr option) (args: Fable.Expr list) =
        match callee with
        | Some callee -> (callee, args)
        | None -> (args.Head, args.Tail)

    let staticArgs (callee: Fable.Expr option) (args: Fable.Expr list) =
        match callee with
        | Some callee -> callee::args
        | None -> args
        
    let emit (i: Fable.ApplyInfo) emit args =
        Fable.Apply(Fable.Emit(emit) |> Fable.Value, args, Fable.ApplyMeth, i.returnType, i.range)

    let emitNoInfo emit args =
        Fable.Apply(Fable.Emit(emit) |> Fable.Value, args, Fable.ApplyMeth, Fable.UnknownType, None)
        
    let wrap typ expr =
        Fable.Wrapped (expr, typ)

    let toString com (i: Fable.ApplyInfo) (arg: Fable.Expr) =
        match arg.Type with
        | Fable.PrimitiveType (Fable.String) -> arg
        | _ -> InstanceCall (arg, "toString", [])
               |> makeCall com i.range i.returnType

    let toInt, toFloat =
        let toNumber com (i: Fable.ApplyInfo) typ (arg: Fable.Expr) =
            match arg.Type with
            | Fable.PrimitiveType Fable.String ->
                GlobalCall ("Number", Some ("parse"+typ), false, [arg])
                |> makeCall com i.range i.returnType
            | _ ->
                if typ = "Int"
                then GlobalCall ("Math", Some "floor", false, [arg])
                     |> makeCall com i.range i.returnType
                else arg
        (fun com i arg -> toNumber com i "Int" arg),
        (fun com i arg -> toNumber com i "Float" arg)

    let toList com (i: Fable.ApplyInfo) expr =
        CoreLibCall ("Seq", Some "toList", false, [expr])
        |> makeCall com i.range i.returnType

    let toArray com (i: Fable.ApplyInfo) expr =
        let dynamicArray expr =
            GlobalCall ("Array", Some "from", false, [expr])
            |> makeCall com i.range (Fable.PrimitiveType(Fable.Array Fable.DynamicArray))
        match expr, i.methodTypeArgs with
        | Fable.Apply(CoreMeth com "List" "ofArray" _, [arr], Fable.ApplyMeth,_,_), _ ->
            arr // Optimization
        | _, [Fable.PrimitiveType(Fable.Number numberKind)] ->
            let arrayKind = Fable.TypedArray numberKind
            Fable.ArrayConst(Fable.ArrayConversion (dynamicArray expr), arrayKind) |> Fable.Value
        | _ -> dynamicArray expr

    let applyOp com (i: Fable.ApplyInfo) (args: Fable.Expr list) meth =
        match args.Head.Type with
        | Fable.UnknownType
        | Fable.PrimitiveType _
        | FullName "System.TimeSpan" ->
            let op =
                match meth with
                | "+" -> Fable.BinaryOp BinaryPlus
                | "-" -> Fable.BinaryOp BinaryMinus
                | "*" -> Fable.BinaryOp BinaryMultiply
                | "/" -> Fable.BinaryOp BinaryDivide
                | "%" -> Fable.BinaryOp BinaryModulus
                | "<<<" -> Fable.BinaryOp BinaryShiftLeft
                | ">>>" -> Fable.BinaryOp BinaryShiftRightSignPropagating
                | "&&&" -> Fable.BinaryOp BinaryAndBitwise
                | "|||" -> Fable.BinaryOp BinaryOrBitwise
                | "^^^" -> Fable.BinaryOp BinaryXorBitwise
                | "~~~" -> Fable.UnaryOp UnaryNotBitwise
                | "~-" -> Fable.UnaryOp UnaryMinus
                | "&&" -> Fable.LogicalOp LogicalAnd
                | "||" -> Fable.LogicalOp LogicalOr
                | _ -> failwithf "Unknown operator: %s" meth
            Fable.Apply(Fable.Value op, args, Fable.ApplyMeth, i.returnType, i.range) |> Some
        | FullName (KeyValue "System.DateTime" "Date" modName)
        | FullName (KeyValue "Microsoft.FSharp.Collections.Set" "Set" modName) ->
            CoreLibCall (modName, Some meth, false, args)
            |> makeCall com i.range i.returnType |> Some
        | Fable.DeclaredType ent ->
            let typRef = Fable.Value (Fable.TypeRef ent)
            InstanceCall(typRef, meth, args)
            |> makeCall com i.range i.returnType |> Some

    let equals com (i: Fable.ApplyInfo) (args: Fable.Expr list) equal =
        let op =
            if equal then BinaryEqualStrict else BinaryUnequalStrict
            |> Fable.BinaryOp |> Fable.Value
        let negateIfNeeded expr =
            if equal then expr
            else makeUnOp i.range i.returnType [expr] UnaryNot  
        match args.Head.Type with
        | Fable.UnknownType
        | Fable.PrimitiveType _ // TODO: Array comparison?
        | FullName "System.TimeSpan" ->
            Fable.Apply(op, args, Fable.ApplyMeth, i.returnType, i.range) |> Some
        | FullName "System.DateTime" ->
            CoreLibCall ("Date", Some "equals", false, args)
            |> makeCall com i.range i.returnType |> negateIfNeeded |> Some
        | Fable.DeclaredType ent ->
            match ent.Kind with
            | Fable.Class _ when ent.HasInterface "System.IComparable" ->
                InstanceCall(args.Head, "equals", args.Tail)
                |> makeCall com i.range i.returnType |> negateIfNeeded |> Some
            // TODO: Record and Union structural equality?
            | _ ->
                Fable.Apply(op, args, Fable.ApplyMeth, i.returnType, i.range) |> Some
            
    let compare com (i: Fable.ApplyInfo) (args: Fable.Expr list) op =
        let op = Fable.BinaryOp op |> Fable.Value
        match args.Head.Type with
        | Fable.UnknownType
        | Fable.PrimitiveType _  // TODO: Array comparison?
        | FullName "System.TimeSpan"
        | FullName "System.DateTime" ->
            Fable.Apply(op, args, Fable.ApplyMeth, i.returnType, i.range) |> Some
        | Fable.DeclaredType ent ->
            match ent.Kind with
            | Fable.Class _ when ent.HasInterface "System.IComparable" ->
                let comp =
                    InstanceCall(args.Head, "compareTo", args.Tail)
                    |> makeCall com i.range (Fable.PrimitiveType (Fable.Number Int32))
                Fable.Apply(op, [comp; makeConst 0], Fable.ApplyMeth, i.returnType, i.range)
                |> Some
            // TODO: Record and Union structural comparison?
            | _ -> None
            
module private AstPass =
    open Util
    
    let fableCore com (i: Fable.ApplyInfo) =
        let destruct = function
            | Fable.Value (Fable.ArrayConst (Fable.ArrayValues exprs, Fable.Tuple)) -> exprs
            | expr -> [expr]
        match i.methodName with
        | "?" ->
            makeGet i.range i.returnType i.args.Head i.args.Tail.Head |> Some
        | "?<-" ->
            match i.callee, i.args with
            | ThreeArgs (callee, prop, value) ->
                Fable.Set (callee, Some prop, value, i.range) |> Some
            | _ -> None
        | "$" ->
            Fable.Apply(i.args.Head, destruct i.args.Tail.Head,
                Fable.ApplyMeth, i.returnType, i.range) |> Some
        | "==>" ->
            (Fable.ArrayValues (List.take 2 i.args), Fable.Tuple)
            |> Fable.ArrayConst |> Fable.Value |> Some
        | "createNew" ->
            Fable.Apply(i.args.Head, destruct i.args.Tail.Head,
                Fable.ApplyCons, i.returnType, i.range) |> Some
        | "createObj" ->
            let (|Fields|_|) = function
                | Fable.Value(Fable.ArrayConst(Fable.ArrayValues exprs, _)) ->
                    exprs
                    |> List.choose (function
                        | Fable.Value
                            (Fable.ArrayConst
                                (Fable.ArrayValues [Fable.Value(Fable.StringConst key); value],
                                    Fable.Tuple)) -> Some(key, value)
                        | _ -> None)
                    |> function
                        | fields when fields.Length = exprs.Length -> Some fields
                        | _ -> None
                | _ -> None
            match i.args.Head with
            | Fable.Apply(_, [Fields fields], _, _, _) ->
                makeJsObject i.range.Value fields |> Some
            | _ ->
                CoreLibCall("Util", Some "createObj", false, i.args)
                |> makeCall com i.range i.returnType |> Some
        | "createEmpty" ->
            Fable.ObjExpr ([], [], i.range)
            |> wrap i.returnType |> Some
        | "areEqual" ->
            ImportCall("assert", true, None, Some "equal", false, i.args)
            |> makeCall com i.range i.returnType |> Some
        | _ -> None
            
    let operators com (info: Fable.ApplyInfo) =
        // TODO: Check primitive args also here?
        let math range typ args methName =
            GlobalCall ("Math", Some methName, false, args)
            |> makeCall com range typ |> Some
        let r, typ, args = info.range, info.returnType, info.args
        match info.methodName with
        // Negation
        | "not" -> makeUnOp r info.returnType args UnaryNot |> Some
        // Equality
        | "<>" | "neq" ->
            match args with
            | [Fable.Value Fable.Null; _]
            | [_; Fable.Value Fable.Null] -> makeEqOp r args BinaryUnequal |> Some
            | _ -> equals com info args false
        | "=" | "eq" ->
            match args with
            | [Fable.Value Fable.Null; _]
            | [_; Fable.Value Fable.Null] -> makeEqOp r args BinaryEqual |> Some
            | _ -> equals com info args true
        // Comparison
        | "<"  | "lt" -> compare com info args BinaryLess
        | "<=" | "lte" -> compare com info args BinaryLessOrEqual
        | ">"  | "gt" -> compare com info args BinaryGreater
        | ">=" | "gte" -> compare com info args BinaryGreaterOrEqual
        // Operators
        | "+" | "-" | "*" | "/" | "%"
        | "<<<" | ">>>" | "&&&" | "|||" | "^^^"
        | "~~~" | "~-" | "&&" | "||" -> applyOp com info args info.methodName
        // Math functions
        // TODO: optimize square pow: x * x
        | "pow" | "pown" | "**" -> math r typ args "pow"
        | "ceil" | "ceiling" -> math r typ args "ceil"
        | "abs" | "acos" | "asin" | "atan" | "atan2" 
        | "cos"  | "exp" | "floor" | "log" | "log10"
        | "round" | "sin" | "sqrt" | "tan" ->
            math r typ args info.methodName
        | "compare" ->
            CoreLibCall("Util", Some "compareTo", false, args)
            |> makeCall com r typ |> Some
        // Function composition
        | ">>" | "<<" ->
            // If expression is a let binding we have to wrap it in a function
            let wrap expr placeholder =
                match expr with
                | Fable.Sequential _ -> sprintf "(function(){return %s}())" placeholder
                | _ -> placeholder
            let args = if info.methodName = ">>" then args else List.rev args
            let f0 = wrap args.Head "$0"
            let f1 = wrap args.Tail.Head "$1"
            emit info (sprintf "x=>%s(%s(x))" f1 f0) args |> Some
        // Reference
        | "!" -> makeGet r Fable.UnknownType args.Head (makeConst "contents") |> Some
        | ":=" -> Fable.Set(args.Head, Some(makeConst "contents"), args.Tail.Head, r) |> Some
        | "ref" -> makeJsObject r.Value [("contents", args.Head)] |> Some
        // Conversions
        | "seq" | "id" | "box" | "unbox" -> wrap typ args.Head |> Some
        | "int" -> toInt com info args.Head |> Some
        | "float" -> toFloat com info args.Head |> Some
        | "char" | "string" -> toString com info args.Head |> Some
        | "dict" | "set" ->
            let modName = if info.methodName = "dict" then "Map" else "Set"
            CoreLibCall(modName, Some "ofSeq", false, args)
            |> makeCall com r typ |> Some
        // Ignore: wrap to keep Unit type (see Fable2Babel.transformFunction)
        | "ignore" -> Fable.Wrapped (args.Head, Fable.PrimitiveType Fable.Unit) |> Some
        // Ranges
        | ".." | ".. .." ->
            let meth = if info.methodName = ".." then "range" else "rangeStep"
            CoreLibCall("Seq", Some meth, false, args)
            |> makeCall com r typ |> Some
        // Tuples
        | "fst" | "snd" ->
            if info.methodName = "fst" then 0 else 1
            |> makeConst
            |> makeGet r typ args.Head |> Some
        // Strings
        | "sprintf" | "printf" | "printfn" | "failwithf" ->
            let emit = 
                match info.methodName with
                | "sprintf" -> "x=>x"
                | "printf" | "printfn" -> "x=>{console.log(x)}"
                | "failwithf" | _ -> "x=>{throw x}" 
                |> Fable.Emit |> Fable.Value
            Fable.Apply(args.Head, [emit], Fable.ApplyMeth, typ, r)
            |> Some
        // Exceptions
        | "failwith" | "raise" | "invalidOp" ->
            Fable.Throw (args.Head, r) |> Some
        // Type ref
        | "typeof" ->
            makeTypeRef com info.range info.methodTypeArgs.Head |> Some
        // Concatenates two lists
        | "@" ->
          CoreLibCall("List", Some "append", false, args)
          |> makeCall com r typ |> Some
        | _ -> None

    let strings com (i: Fable.ApplyInfo) =
        let icall meth =
            let c, args = instanceArgs i.callee i.args
            InstanceCall(c, meth, args)
            |> makeCall com i.range i.returnType
        match i.methodName with
        | ".ctor" ->
            CoreLibCall("String", Some "fsFormat", false, i.args)
            |> makeCall com i.range i.returnType |> Some
        | "length" ->
            let c, _ = instanceArgs i.callee i.args
            makeGet i.range i.returnType c (makeConst "length") |> Some
        | "contains" ->
            makeEqOp i.range [icall "indexOf"; makeConst 0] BinaryGreaterOrEqual |> Some
        | "startsWith" ->
            makeEqOp i.range [icall "indexOf"; makeConst 0] BinaryEqualStrict |> Some
        | "substring" -> icall "substr" |> Some
        | "toUpper" -> icall "toLocaleUpperCase" |> Some
        | "toUpperInvariant" -> icall "toUpperCase" |> Some
        | "toLower" -> icall "toLocaleLowerCase" |> Some
        | "toLowerInvariant" -> icall "toLowerCase" |> Some
        | "indexOf" | "lastIndexOf" | "trim" -> icall i.methodName |> Some
        | "toCharArray" ->
            InstanceCall(i.callee.Value, "split", [makeConst ""])
            |> makeCall com i.range i.returnType |> Some
        | "iter" | "iteri" | "forall" | "exists" ->
            CoreLibCall("Seq", Some i.methodName, false, deleg i.args)
            |> makeCall com i.range i.returnType |> Some
        | "map" | "mapi" | "collect"  ->
            CoreLibCall("Seq", Some i.methodName, false, deleg i.args)
            |> makeCall com i.range Fable.UnknownType
            |> List.singleton
            |> emit i "Array.from($0).join('')"
            |> Some
        | _ -> None

    let console com (i: Fable.ApplyInfo) =
        match i.methodName with
        | "write" | "writeLine" ->
            let inner =
                CoreLibCall("String", Some "format", false, i.args)
                |> makeCall com i.range (Fable.PrimitiveType Fable.String)
            GlobalCall("console", Some "log", false, [inner])
            |> makeCall com i.range i.returnType |> Some
        | "assert" -> failwith "TODO: Assertions"
        | _ -> None

    let regex com (i: Fable.ApplyInfo) =
        let prop p callee =
            makeGet i.range i.returnType callee (makeConst p)
        let isGroup =
            match i.callee with
            | Some (Type (FullName "System.Text.RegularExpressions.Group")) -> true
            | _ -> false
        match i.methodName with
        | ".ctor" ->
            // TODO: Use RegexConst if no options have been passed?
            CoreLibCall("RegExp", Some "create", false, i.args)
            |> makeCall com i.range i.returnType |> Some
        | "options" ->
            CoreLibCall("RegExp", Some "options", false, [i.callee.Value])
            |> makeCall com i.range i.returnType |> Some
        // Capture
        | "index" ->
            if isGroup
            then failwithf "Accessing index of Regex groups is not supported"
            else prop "index" i.callee.Value |> Some
        | "value" ->
            if isGroup
            then i.callee.Value |> wrap i.returnType |> Some
            else prop 0 i.callee.Value |> Some
        | "length" ->
            if isGroup
            then prop "length" i.callee.Value |> Some
            else prop 0 i.callee.Value |> prop "length" |> Some
        // Group
        | "success" ->
            makeEqOp i.range [i.callee.Value; Fable.Value Fable.Null] BinaryUnequal |> Some
        // Match
        | "groups" -> wrap i.returnType i.callee.Value |> Some
        // MatchCollection & GroupCollection
        | "item" ->
            makeGet i.range i.returnType i.callee.Value i.args.Head |> Some
        | "count" ->
            prop "length" i.callee.Value |> Some
        | _ -> None

    let intrinsicFunctions com (i: Fable.ApplyInfo) =
        match i.methodName, (i.callee, i.args) with
        | "unboxGeneric", OneArg (arg) -> wrap i.returnType arg |> Some
        | "getString", TwoArgs (ar, idx)
        | "getArray", TwoArgs (ar, idx) ->
            makeGet i.range i.returnType ar idx |> Some
        | "setArray", ThreeArgs (ar, idx, value) ->
            Fable.Set (ar, Some idx, value, i.range) |> Some
        | "getArraySlice", ThreeArgs (ar, lower, upper) ->
            let upper =
                match upper with
                | Null _ -> emitNoInfo "$0.length" [ar]
                | _ -> emitNoInfo "$0 + 1" [upper]
            InstanceCall (ar, "slice", [lower; upper])
            |> makeCall com i.range i.returnType |> Some
        | "setArraySlice", (None, args) ->
            CoreLibCall("Array", Some "setSlice", false, args)
            |> makeCall com i.range i.returnType |> Some
        | "createInstance", (None, args) ->
            let typRef, args =
                match args with
                | [] | [Fable.Value Fable.Null] ->
                    makeTypeRef com i.range i.methodTypeArgs.Head, []
                | typRef::args -> typRef, args
            Fable.Apply (typRef, args, Fable.ApplyCons, i.returnType, i.range) |> Some
        | _ -> None

    let options com (i: Fable.ApplyInfo) =
        let toArray r optExpr =
            Fable.Apply(Fable.Emit("$0 != null ? [$0]: []") |> Fable.Value, [optExpr],
                Fable.ApplyMeth, Fable.PrimitiveType (Fable.Array Fable.DynamicArray), r)
        let callee = match i.callee with Some c -> c | None -> i.args.Head
        match i.methodName with
        | "value" | "get" | "toObj" | "ofObj" | "toNullable" | "ofNullable" ->
           wrap i.returnType callee |> Some
        | "isSome" -> makeEqOp i.range [callee; Fable.Value Fable.Null] BinaryUnequal |> Some
        | "isNone" -> makeEqOp i.range [callee; Fable.Value Fable.Null] BinaryEqual |> Some
        | "map" | "bind" -> emit i "$1 != null ? $0($1) : $1" i.args |> Some
        | "toArray" -> toArray i.range i.args.Head |> Some
        | meth ->
            let args =
                let args = List.rev i.args
                (toArray i.range args.Head)::args.Tail |> List.rev
            CoreLibCall("Seq", Some meth, false, deleg args)
            |> makeCall com i.range i.returnType |> Some
        
    let timeSpans com (i: Fable.ApplyInfo) =
        // let callee = match i.callee with Some c -> c | None -> i.args.Head
        match i.methodName with
        | ".ctor" ->
            CoreLibCall("TimeSpan", Some "create", false, i.args)
            |> makeCall com i.range i.returnType |> Some
        | "fromMilliseconds" ->
            wrap i.returnType i.args.Head |> Some
        | "totalMilliseconds" ->
            wrap i.returnType i.callee.Value |> Some
        | _ -> None

    let dates com (i: Fable.ApplyInfo) =
        match i.methodName with
        | ".ctor" ->
            let args =
                let last = List.last i.args
                match i.args.Length, last.Type with
                | 7, FullName "System.DateTimeKind" ->
                    (List.take 6 i.args)@[makeConst 0; last]
                | _ -> i.args
            CoreLibCall("Date", Some "create", false, args)
            |> makeCall com i.range i.returnType |> Some
        | "kind" ->
            makeGet i.range i.returnType i.callee.Value (makeConst "kind")
            |> Some
        | _ -> None
        
    let rawCollections com (i: Fable.ApplyInfo) =
        match i.methodName with
        | "count" ->
            CoreLibCall ("Seq", Some "length", false, [i.callee.Value])
            |> makeCall com i.range i.returnType |> Some
        | _ -> None

    let keyValuePairs com (i: Fable.ApplyInfo) =
        let get (k: obj) =
            makeGet i.range i.returnType i.callee.Value (makeConst k) |> Some
        match i.methodName with
        | "key" -> get 0
        | "value" -> get 1
        | _ -> None

    let dictionaries com (i: Fable.ApplyInfo) =
        let icall meth =
            InstanceCall (i.callee.Value, meth, i.args)
            |> makeCall com i.range i.returnType |> Some
        match i.methodName with
        | ".ctor" ->
            let makeMap args =
                GlobalCall("Map", None, true, args) |> makeCall com i.range i.returnType
            match i.args with
            | [] -> makeMap [] |> Some
            | _ ->
                match i.args.Head.Type with
                | Fable.PrimitiveType (Fable.Number Int32) ->
                    makeMap [] |> Some
                | _ -> makeMap i.args |> Some
        | "isReadOnly" ->
            Fable.BoolConst false |> Fable.Value |> Some
        | "count" ->
            makeGet i.range i.returnType i.callee.Value (makeConst "size") |> Some
        | "containsValue" ->
            CoreLibCall ("Map", Some "containsValue", false, [i.args.Head; i.callee.Value])
            |> makeCall com i.range i.returnType |> Some
        | "item" -> icall "get"
        | "keys" -> icall "keys"
        | "values" -> icall "values"
        | "containsKey" -> icall "has"
        | "clear" -> icall "clear"
        | "add" -> icall "set"
        | "remove" -> icall "delete"
        | _ -> None

    let mapAndSets com (i: Fable.ApplyInfo) =
        let instanceArgs () =
            match i.callee with
            | Some c -> c, i.args
            | None -> List.last i.args, List.take (i.args.Length-1) i.args
        let prop (prop: string) =
            let callee, _ = instanceArgs()
            makeGet i.range i.returnType callee (makeConst prop)
        let icall meth =
            let callee, args = instanceArgs()
            InstanceCall (callee, meth, args)
            |> makeCall com i.range i.returnType
        let modName =
            if i.ownerFullName.EndsWith("Map")
            then "Map" else "Set"
        let _of colType expr =
            CoreLibCall(modName, Some ("of" + colType), false, [expr])
            |> makeCall com i.range i.returnType
        match i.methodName with
        // Instance and static shared methods
        | "add" ->
            match modName with
            | "Map" -> icall "set" |> Some
            | _ -> icall "add" |> Some
        | "count" -> prop "size" |> Some
        | "contains" | "containsKey" -> icall "has" |> Some
        | "remove" ->
            let callee, args = instanceArgs()
            CoreLibCall(modName, Some "removeInPlace", false, [args.Head; callee])
            |> makeCall com i.range i.returnType |> Some
        | "isEmpty" ->
            makeEqOp i.range [prop "size"; makeConst 0] BinaryEqualStrict |> Some
        // Map only instance and static methods
        | "tryFind" | "find" -> icall "get" |> Some
        | "item" -> icall "get" |> Some
        // Set only instance and static methods
        | KeyValue "maximumElement" "max" meth | KeyValue "maxElement" "max" meth
        | KeyValue "minimumElement" "min" meth | KeyValue "minElement" "min" meth ->
            let args = match i.callee with Some x -> [x] | None -> i.args
            CoreLibCall("Seq", Some meth, false, args)
            |> makeCall com i.range i.returnType |> Some
        // Constructors
        | "empty" ->
            GlobalCall(modName, None, true, [])
            |> makeCall com i.range i.returnType |> Some
        | ".ctor" ->
            CoreLibCall(modName, Some "ofSeq", false, i.args)
            |> makeCall com i.range i.returnType |> Some
        // Conversions
        | "toArray" -> toArray com i i.args.Head |> Some
        | "toList" -> toList com i i.args.Head |> Some
        | "toSeq" -> Some i.args.Head
        | "ofArray" -> _of "Array" i.args.Head |> Some
        | "ofList" | "ofSeq" -> _of "Seq" i.args.Head |> Some
        // Non-build static methods shared with Seq
        | "exists" | "fold" | "foldBack" | "forall" | "iter" ->
            let modName = if modName = "Map" then "Map" else "Seq"
            CoreLibCall(modName, Some i.methodName, false, deleg i.args)
            |> makeCall com i.range i.returnType |> Some
        // Build static methods shared with Seq
        | "filter" | "map" ->
            match modName with
            | "Map" ->
                CoreLibCall(modName, Some i.methodName, false, deleg i.args)
                |> makeCall com i.range i.returnType |> Some
            | _ ->
                CoreLibCall("Seq", Some i.methodName, false, deleg i.args)
                |> makeCall com i.range i.returnType
                |> _of "Seq" |> Some
        // Static method
        | "partition" ->
            CoreLibCall(modName, Some i.methodName, false, deleg i.args)
            |> makeCall com i.range i.returnType |> Some
        // Map only static methods (make delegate)
        | "findKey" | "tryFindKey" | "pick" | "tryPick" ->
            CoreLibCall("Map", Some i.methodName, false, deleg i.args)
            |> makeCall com i.range i.returnType |> Some
        // Set only static methods
        | "singleton" ->
            emit i "new Set([$0])" i.args |> Some
        | _ -> None

    type CollectionKind =
        | Seq | List | Array
    
    // Functions which don't return a new collection of the same type
    let implementedSeqNonBuildFunctions =
        set [ "average"; "averageBy"; "countBy"; "compareWith"; "empty";
              "exactlyOne"; "exists"; "exists2"; "fold"; "fold2"; "foldBack"; "foldBack2";
              "forall"; "forall2"; "head"; "item"; "iter"; "iteri"; "iter2"; "iteri2";
              "isEmpty"; "last"; "length"; "max"; "maxBy"; "min"; "minBy";
              "reduce"; "reduceBack"; "sum"; "sumBy"; "tail"; "toList";
              "tryFind"; "find"; "tryFindIndex"; "findIndex"; "tryPick"; "pick"; "unfold" ]

    // Functions that must return a collection of the same type
    let implementedSeqBuildFunctions =
        set [ "append"; "choose"; "collect"; "concat"; "distinctBy"; "distinctBy";
              "filter"; "where"; "groupBy"; "init";
              "map"; "mapi"; "map2"; "mapi2"; "map3";
              "ofArray"; "pairwise"; "permute"; "replicate"; "rev";
              "scan"; "scanBack"; "singleton"; "skip"; "skipWhile";
              "take"; "takeWhile"; "sort"; "sortBy"; "sortWith";
              "sortDescending"; "sortByDescending"; "zip"; "zip3" ]

    let implementedListFunctions =
        set [ "append"; "choose"; "collect"; "concat"; "filter"; "where";
              "init"; "map"; "mapi"; "ofArray"; "partition";
              "replicate"; "rev"; "singleton"; "unzip"; "unzip3" ]

    let implementedArrayFunctions =
        set [ "partition"; "permute"; "sortInPlaceBy"; "unzip"; "unzip3" ]
        
    let nativeArrayFunctions =
        dict [ "exists" => "some"; "filter" => "filter";
               "find" => "find"; "findIndex" => "findIndex"; "forall" => "every";
               "indexed" => "entries"; "iter" => "forEach"; "map" => "map";
               "reduce" => "reduce"; "reduceBack" => "reduceRight";
               "sortInPlace" => "sort"; "sortInPlaceWith" => "sort" ]

    let collectionsSecondPass com (i: Fable.ApplyInfo) kind =
        let prop (meth: string) callee =
            makeGet i.range i.returnType callee (makeConst meth)
        let icall meth (callee, args) =
            InstanceCall (callee, meth, args)
            |> makeCall com i.range i.returnType
        let ccall modName meth args =
            CoreLibCall (modName, Some meth, false, args)
            |> makeCall com i.range i.returnType
        let meth, c, args =
            i.methodName, i.callee, i.args
        match meth with
        // Deal with special cases first
        // | "sum" | "sumBy" -> // TODO: Check if we need to use a custom operator
        | "cast" -> Some i.args.Head // Seq only, erase
        | "isEmpty" ->
            match kind with
            | Seq -> ccall "Seq" meth args
            | Array ->
                makeEqOp i.range [prop "length" args.Head; makeConst 0] BinaryEqualStrict
            | List ->
                let c, _ = instanceArgs c args
                makeEqOp i.range [prop "tail" c; Fable.Value Fable.Null] BinaryEqual
            |> Some
        | "head" | "tail" | "length" | "count" ->
            match kind with
            | Seq -> ccall "Seq" meth (staticArgs c args)
            | List -> let c, _ = instanceArgs c args in prop meth c
            | Array ->
                let c, _ = instanceArgs c args
                if meth = "head" then makeGet i.range i.returnType c (makeConst 0)
                elif meth = "tail" then icall "slice" (c, [makeConst 1])
                else prop "length" c
            |> Some
        | "item" ->
            match i.callee, kind with
            | Some callee, Array ->
                if i.args.Length = 1
                then makeGet i.range i.returnType callee i.args.Head
                else Fable.Set (i.callee.Value, Some i.args.Head, i.args.Tail.Head, i.range)
            | _, Seq -> ccall "Seq" meth args
            | _, Array -> makeGet i.range i.returnType args.Tail.Head args.Head
            | _, List -> match i.callee with Some x -> i.args@[x] | None -> i.args
                         |> ccall "Seq" meth
            |> Some
        | "sort" ->
            match c, kind with
            | Some c, _ -> icall "sort" (c, deleg args)
            | None, Seq -> ccall "Seq" meth (deleg args)
            | None, List -> ccall "Seq" meth (deleg args) |> toList com i
            | None, Array -> ccall "Seq" meth (deleg args) |> toArray com i
            |> Some
        // Constructors ('cons' only applies to List)
        | "empty" | "cons" ->
            match kind with
            | Seq -> ccall "Seq" meth args
            | Array ->
                match i.returnType with
                | Fable.PrimitiveType (Fable.Array kind) ->
                    Fable.ArrayConst (Fable.ArrayAlloc (makeConst 0), kind) |> Fable.Value
                | _ -> failwithf "Expecting array type but got %A" i.returnType
            | List -> CoreLibCall ("List", None, true, args)
                      |> makeCall com i.range i.returnType
            |> Some
        | "zeroCreate" ->
            match i.methodTypeArgs with
            | [Fable.PrimitiveType(Fable.Number numberKind)] ->
                Fable.ArrayConst(Fable.ArrayAlloc i.args.Head, Fable.TypedArray numberKind)
                |> Fable.Value |> Some
            | _ -> failwithf "Unexpected arguments for Array.zeroCreate: %A" i.args
        // ResizeArray only
        | ".ctor" ->
            let makeEmptyArray () =
                (Fable.ArrayValues [], Fable.DynamicArray)
                |> Fable.ArrayConst |> Fable.Value |> Some
            match i.args with
            | [] -> makeEmptyArray ()
            | _ ->
                match i.args.Head.Type with
                | Fable.PrimitiveType (Fable.Number Int32) ->
                    makeEmptyArray ()
                | _ -> emit i "Array.from($0)" i.args |> Some
        | "add" ->
            icall "push" (c.Value, args) |> Some
        | "addRange" ->
            ccall "Array" "addRangeInPlace" [args.Head; c.Value] |> Some
        | "clear" ->
            icall "splice" (c.Value, [makeConst 0]) |> Some
        | "contains" ->
            emit i "$0.indexOf($1) > -1" (c.Value::args) |> Some
        | "indexOf" ->
            icall "indexOf" (c.Value, args) |> Some
        | "insert" ->
            icall "splice" (c.Value, [args.Head; makeConst 0; args.Tail.Head]) |> Some
        | "remove" ->
            ccall "Array" "removeInPlace" [args.Head; c.Value] |> Some
        | "removeAt" ->
            icall "splice" (c.Value, [args.Head; makeConst 1]) |> Some
        | "reverse" ->
            icall "reverse" (c.Value, []) |> Some
        // Conversions
        | "toSeq" | "ofSeq" ->
            match kind with
            | Seq -> failwithf "Unexpected method called on seq %s in %A" meth i.range
            | List -> ccall "Seq" (if meth = "toSeq" then "ofList" else "toList") args
            | Array ->
                if meth = "toSeq"
                then ccall "Seq" "ofArray" args
                else toArray com i args.Head
            |> Some
        | "toArray" ->
            toArray com i i.args.Head |> Some
        | "ofList" ->
            match kind with
            | List -> failwithf "Unexpected method called on list %s in %A" meth i.range
            | Seq -> ccall "Seq" "ofList" args
            | Array -> toArray com i i.args.Head
            |> Some
        // Default to Seq implementation in core lib
        | SetContains implementedSeqNonBuildFunctions meth ->
            ccall "Seq" meth (deleg args) |> Some
        | SetContains implementedSeqBuildFunctions meth ->
            match kind with
            | Seq -> ccall "Seq" meth (deleg args)
            | List -> ccall "Seq" meth (deleg args) |> toList com i
            | Array -> ccall "Seq" meth (deleg args) |> toArray com i
            |> Some
        | _ -> None
        
    let collectionsFirstPass com (i: Fable.ApplyInfo) kind =
        match kind with
        | List ->
            match i.methodName with
            | "getSlice" ->
                InstanceCall (i.callee.Value, "slice", i.args) |> Some
            | SetContains implementedListFunctions meth ->
                CoreLibCall ("List", Some meth, false, deleg i.args) |> Some
            | _ -> None
        | Array ->
            match i.methodName with
            | "take" ->
                InstanceCall (i.args.Tail.Head, "slice", [makeConst 0; i.args.Head]) |> Some
            | "skip" ->
                InstanceCall (i.args.Tail.Head, "slice", [i.args.Head]) |> Some
            | SetContains implementedArrayFunctions meth ->
                CoreLibCall ("Array", Some meth, false, deleg i.args) |> Some
            | DicContains nativeArrayFunctions meth ->
                let revArgs = List.rev i.args
                InstanceCall (revArgs.Head, meth, deleg (List.rev revArgs.Tail)) |> Some
            | _ -> None
        | _ -> None
        |> function
            | Some callKind -> makeCall com i.range i.returnType callKind |> Some
            | None -> collectionsSecondPass com i kind

    let asserts com (i: Fable.ApplyInfo) =
        match i.methodName with
        | "areEqual" ->
            ImportCall("assert", true, None, Some "equal", false, i.args)
            |> makeCall com i.range i.returnType |> Some
        | _ -> None
        
    let exceptions com (i: Fable.ApplyInfo) =
        match i.methodName with
        | ".ctor" ->
            match i.args with
            | [] -> Fable.ObjExpr ([], [], i.range)
            | [arg] -> arg
            | args -> makeArray Fable.UnknownType args
            |> Some
        | "message" -> i.callee
        | _ -> None
        
    let cancels com (i: Fable.ApplyInfo) =
        match i.methodName with
        | ".ctor" -> Fable.ObjExpr ([], [], i.range) |> Some
        | "token" -> i.callee
        | "cancel" -> emit i "$0.isCancelled = true" [i.callee.Value] |> Some
        | "cancelAfter" -> emit i "setTimeout(function () { $0.isCancelled = true }, $1)" [i.callee.Value; i.args.Head] |> Some
        | "isCancellationRequested" -> emit i "$0.isCancelled" [i.callee.Value] |> Some 
        | _ -> None

    let knownInterfaces com (i: Fable.ApplyInfo) =
        match i.methodName with
        | ".ctor" -> Fable.ObjExpr ([], [], i.range) |> Some
        | meth -> InstanceCall (i.callee.Value, meth, i.args)
                  |> makeCall com i.range i.returnType |> Some

    let tryReplace com (info: Fable.ApplyInfo) =
        match info.ownerFullName with
        | KnownInterfaces _ -> knownInterfaces com info
        | Naming.StartsWith "Fable.Core" _ -> fableCore com info
        | "System.String"
        | "Microsoft.FSharp.Core.String"
        | "Microsoft.FSharp.Core.PrintfFormat" -> strings com info
        | "System.Console"
        | "System.Diagnostics.Debug" -> console com info
        | "System.DateTime" -> dates com info 
        | "System.TimeSpan" -> timeSpans com info 
        | "Microsoft.FSharp.Core.Option" -> options com info
        | "Microsoft.FSharp.Core.MatchFailureException"
        | "System.Exception" -> exceptions com info
        | "System.Threading.CancellationToken"
        | "System.Threading.CancellationTokenSource" -> cancels com info
        | "System.Math"
        | "Microsoft.FSharp.Core.Operators"
        | "Microsoft.FSharp.Core.ExtraTopLevelOperators" -> operators com info
        | "System.Activator"
        | "Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions"
        | "Microsoft.FSharp.Core.Operators.OperatorIntrinsics" -> intrinsicFunctions com info
        | "System.Text.RegularExpressions.Capture"
        | "System.Text.RegularExpressions.Match"
        | "System.Text.RegularExpressions.Group"
        | "System.Text.RegularExpressions.MatchCollection"
        | "System.Text.RegularExpressions.GroupCollection"
        | "System.Text.RegularExpressions.Regex" -> regex com info
        | "System.Collections.Generic.Dictionary"
        | "System.Collections.Generic.IDictionary" -> dictionaries com info
        | "System.Collections.Generic.KeyValuePair" -> keyValuePairs com info 
        | "System.Collections.Generic.Dictionary`2.KeyCollection"
        | "System.Collections.Generic.Dictionary`2.ValueCollection"
        | "System.Collections.Generic.ICollection" -> rawCollections com info
        | "System.Array"
        | "System.Collections.Generic.List"
        | "System.Collections.Generic.IList" -> collectionsSecondPass com info Array
        | "Microsoft.FSharp.Collections.Array" -> collectionsFirstPass com info Array
        | "Microsoft.FSharp.Collections.List" -> collectionsFirstPass com info List
        | "Microsoft.FSharp.Collections.Seq" -> collectionsSecondPass com info Seq
        | "Microsoft.FSharp.Collections.Map"
        | "Microsoft.FSharp.Collections.Set" -> mapAndSets com info
        | _ -> None

module private CoreLibPass =
    open Util

    type MapKind = Static | Both

    let mappings =
        dict [
            system + "DateTime" => ("Date", Static)
            system + "TimeSpan" => ("TimeSpan", Static)
            fsharp + "Control.Async" => ("Async", Both)
            fsharp + "Control.AsyncBuilder" => ("Async", Both)
            fsharp + "Control.Observable" => ("Observable", Static)
            fsharp + "Core.CompilerServices.RuntimeHelpers" => ("Seq", Static)
            system + "String" => ("String", Static)
            fsharp + "Core.String" => ("String", Static)
            system + "Text.RegularExpressions.Regex" => ("RegExp", Static)
            fsharp + "Collections.Seq" => ("Seq", Static)
            fsharp + "Collections.Set" => ("Set", Static)
            fsharp + "Core.Choice" => ("Choice", Both)
        ]

open Util

let private coreLibPass com (info: Fable.ApplyInfo) =
    match info.ownerFullName with
    | DicContains CoreLibPass.mappings (modName, kind) ->
        match kind with
        | CoreLibPass.Both ->
            match info.methodName, info.callee with
            | ".ctor", None ->
                CoreLibCall(modName, None, true, deleg info.args)
                |> makeCall com info.range info.returnType
            | _, Some callee ->
                InstanceCall (callee, info.methodName, deleg info.args)
                |> makeCall com info.range info.returnType
            | _, None ->
                CoreLibCall(modName, Some info.methodName, false, staticArgs info.callee info.args |> deleg)
                |> makeCall com info.range info.returnType
        | CoreLibPass.Static ->
            let meth = if info.methodName = ".ctor" then "create" else info.methodName
            CoreLibCall(modName, Some meth, false, staticArgs info.callee info.args |> deleg)
            |> makeCall com info.range info.returnType
        |> Some
    | _ -> None

let tryReplace (com: ICompiler) (info: Fable.ApplyInfo) =
    match AstPass.tryReplace com info with
    | Some res -> Some res
    | None -> coreLibPass com info
