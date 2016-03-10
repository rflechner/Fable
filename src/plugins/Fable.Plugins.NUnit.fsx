namespace Fable.Plugins

#r "../../build/fable/bin/Fable.exe"

open Fable.AST
open Fable.FSharp2Fable
open Fable.Fable2Babel

module Util =
    let (|TestFixture|_|) (decl: Fable.Declaration) =
        match decl with
        | Fable.EntityDeclaration (ent, entDecls, entRange) ->
            match ent.TryGetDecorator "TestFixture" with
            | Some _ -> Some (ent, entDecls, entRange)
            | None -> None
        | _ -> None

    let (|Test|_|) (decl: Fable.Declaration) =
        match decl with
        | Fable.MemberDeclaration m ->
            match m.Kind, m.TryGetDecorator "Test" with
            | Fable.Method name, Some _ -> Some (m, name)
            | _ -> None
        | _ -> None

    // Compile tests using Mocha.js BDD interface
    let transformTest com ctx (test: Fable.Member) name =
        let testName =
            Babel.StringLiteral name :> Babel.Expression
        let testBody =
            Util.funcExpression com ctx test.Arguments test.Body :> Babel.Expression
        let testRange =
            match testBody.loc with
            | Some loc -> test.Range + loc | None -> test.Range
        // it('Test name', function() { /* Tests */ });
        Babel.ExpressionStatement(
            Babel.CallExpression(Babel.Identifier "it",
                [U2.Case1 testName; U2.Case1 testBody], testRange), testRange)
        :> Babel.Statement

    let transformTestFixture com ctx (fixture: Fable.Entity) testDecls testRange =
        let testDesc =
            Babel.StringLiteral fixture.Name :> Babel.Expression
        let testBody =
            Babel.FunctionExpression([],
                Babel.BlockStatement (testDecls, ?loc=Some testRange), ?loc=Some testRange)
            :> Babel.Expression
        Babel.ExpressionStatement(
            Babel.CallExpression(Babel.Identifier "describe",
                [U2.Case1 testDesc; U2.Case1 testBody],
                testRange)) :> Babel.Statement

    let asserts com (i: Fable.ApplyInfo) =
        match i.methodName with
        | "areEqual" ->
            Fable.Util.ImportCall("assert", true, None, Some "equal", false, i.args)
            |> Fable.Util.makeCall com i.range i.returnType |> Some
        | _ -> None

open Util

type NUnitPlugin() =
    interface IDeclarePlugin with
        member x.TryDeclareRoot com ctx file =
            if file.Root.TryGetDecorator "TestFixture" |> Option.isNone then None else
            let rootDecls = Util.transformModDecls com ctx None file.Declarations
            transformTestFixture com ctx file.Root rootDecls file.Range
            |> U2.Case1
            |> List.singleton
            |> Some
        member x.TryDeclare com ctx decl =
            match decl with
            | Test (test, name) ->
                transformTest com ctx test name
                |> List.singleton |> Some
            | TestFixture (fixture, testDecls, testRange) ->
                let testDecls =
                    let ctx = { ctx with moduleFullName = fixture.FullName } 
                    Util.transformModDecls com ctx None testDecls
                transformTestFixture com ctx fixture testDecls testRange
                |> List.singleton |> Some
            | _ -> None

    interface IReplacePlugin with
        member x.TryReplace com info =
            match info.ownerFullName with
            | "NUnit.Framework.Assert" -> asserts com info
            | _ -> None
