﻿[<NUnit.Framework.TestFixture>]
module Fable.Tests.TypeTests

open NUnit.Framework
open Fable.Tests.Util

type ITest = interface end
type ITest2 = interface end

type TestType =
    | Union1 of string
    interface ITest

type TestType2 =
    | Union2 of string
    interface ITest
    
type TestType3() =
    member __.Value = "Hi"

type TestType4() =
    member __.Value = "Bye"

type TestType5(greeting: string) =
    member __.Value = greeting
    member __.Overload(x) = x + x
    member __.Overload(x, y) = x + y
    
let normalize (x: string) =
    #if MOCHA
    x
    #else
    x.Replace("+",".")
    #endif

[<Test>]
let ``Type Namespace``() =
    let x = typeof<TestType>.Namespace
    #if MOCHA
    equal "Fable.Tests.TypeTests" x
    #else
    equal "Fable.Tests" x
    #endif    

[<Test>]
let ``Type FullName``() =
    let x = typeof<TestType>.FullName
    x |> normalize |> equal "Fable.Tests.TypeTests.TestType"

[<Test>]
let ``Type Name``() =
    let x = typeof<TestType>.Name
    equal "TestType" x

[<Test>]
let ``Type testing``() =
    let x = Union1 "test" :> obj
    let y = Union2 "test" :> obj
    x :? TestType |> equal true
    x :? TestType2 |> equal false
    y :? TestType |> equal false
    y :? TestType2 |> equal true

[<Test>]
let ``Interface testing``() =
    let x = Union1 "test" :> obj
    let y = Union2 "test" :> obj
    x :? ITest |> equal true
    x :? ITest2 |> equal false
    y :? ITest |> equal true
    y :? ITest2 |> equal false
    
[<Test>]
let ``Type testing in pattern matching``() =
    let x = Union1 "test" :> obj
    match x with
    | :? TestType as x -> let (Union1 str) = x in str
    | _ -> "FAIL"
    |> equal "test"
    match x with
    | :? TestType2 as x -> let (Union2 str) = x in str
    | _ -> "FAIL"
    |> equal "FAIL"

[<Test>]
let ``Interface testing in pattern matching``() =
    let x = Union2 "test" :> obj
    match x with | :? ITest -> true | _ -> false
    |> equal true
    match x with | :? ITest2 -> true | _ -> false
    |> equal false

let inline fullname<'T> () = typeof<'T>.FullName |> normalize

[<Test>]
let ``Get fullname of generic types with inline function``() =
    fullname<TestType3>() |> equal "Fable.Tests.TypeTests.TestType3"
    fullname<TestType4>() |> equal "Fable.Tests.TypeTests.TestType4"

let inline create<'T when 'T:(new : unit -> 'T)> () = new 'T()
    
[<Test>]
let ``Create new generic objects with inline function``() =
    create<TestType3>().Value |> equal "Hi"
    create<TestType4>().Value |> equal "Bye"
    // create<TestType5>() // Doesn't compile    

let inline create2<'T> (args: obj[]) =
    System.Activator.CreateInstance(typeof<'T>, args) :?> 'T
    
[<Test>]
let ``Create new generic objects with System.Activator``() =
    (create2<TestType3> [||]).Value |> equal "Hi"
    (create2<TestType4> [||]).Value |> equal "Bye"
    (create2<TestType5> [|"Yo"|]).Value |> equal "Yo"
        
type RenderState = 
    { Now : int
      Players : Map<int, string>
      Map : string }

[<Test>]
let ``Property names don't clash with built-in JS objects``() = // See #168
    let gameState = {
        Now = 1
        Map = "dungeon"
        Players = Map.empty 
    } 
    gameState.Players.ContainsKey(1) |> equal false

[<Test>]
let ``Overloads work``() =
    let t = TestType5("")
    t.Overload(2) |> equal 4
    t.Overload(2, 3) |> equal 5
    
type T4 = TestType4

[<Test>]
let ``Type abbreviation works``() =
    let t = T4()
    t.Value |> equal "Bye"

type TestType6(x: int) =
    let mutable i = x
    member val Value1 = i with get, set
    member __.Value2 = i + i
    member __.Value3 with get() = i * i and set(v) = i <- v

[<Test>]
let ``Getter and Setter work``() =
    let t = TestType6(5)
    t.Value1 |> equal 5
    t.Value2 |> equal 10
    t.Value3 |> equal 25
    t.Value3 <- 10
    t.Value1 |> equal 5
    t.Value2 |> equal 20
    t.Value3 |> equal 100
    t.Value1 <- 20
    t.Value1 |> equal 20
    t.Value2 |> equal 20
    t.Value3 |> equal 100

type TestType7(a1, a2, a3) =
    let arr = [|a1; a2; a3|]
    member __.Value with get(i) = arr.[i] and set(i) (v) = arr.[i] <- v 

[<Test>]
let ``Getter and Setter with indexer work``() =
    let t = TestType7(1, 2, 3)
    t.Value(1) |> equal 2
    t.Value(2) |> equal 3
    t.Value(1) <- 5
    t.Value(1) |> equal 5
    t.Value(2) |> equal 3
