[<NUnit.Framework.TestFixture>] 
module Fable.Tests.Async
open System
open NUnit.Framework
open Fable.Tests.Util

[<Test>]
let ``Simple async translates without exception``() =
    async { return () }
    |> Async.StartImmediate

[<Test>]
let ``Async while binding works correctly``() =
    let mutable result = 0
    async { 
        while result < 10 do
            result <- result + 1
    } |> Async.StartImmediate
    equal result 10

[<Test>]
let ``Async for binding works correctly``() =
    let inputs = [|1; 2; 3|]
    let result = ref 0
    async { 
        for inp in inputs do
            result := !result + inp
    } |> Async.StartImmediate
    equal !result 6

[<Test>]
let ``Async exceptions are handled correctly``() =
    let result = ref 0
    let f shouldThrow =
        async { 
            try
                if shouldThrow then failwith "boom!"
                else result := 12
            with _ -> result := 10
        } |> Async.StartImmediate
        !result
    f true + f false |> equal 22

[<Test>]
let ``Simple async is executed correctly``() =
    let result = ref false
    let x = async { return 99 }
    async { 
        let! x = x
        let y = 99
        result := x = y
    }
    //TODO: RunSynchronously would make more sense here but in JS I think this will be ok.
    |> Async.StartImmediate 
    equal !result true

type DisposableAction(f) =
    interface IDisposable with
        member __.Dispose() = f()

[<Test>]
let ``async use statements should dispose of resources when they go out of scope``() =
    let isDisposed = ref false
    let step1ok = ref false
    let step2ok = ref false
    let resource = async {
        return new DisposableAction(fun () -> isDisposed := true)
    }
    async { 
        use! r = resource
        step1ok := not !isDisposed
    }
    //TODO: RunSynchronously would make more sense here but in JS I think this will be ok.
    |> Async.StartImmediate 
    step2ok := !isDisposed
    (!step1ok && !step2ok) |> equal true

[<Test>]
let ``Try ... with ... expressions inside async expressions work the same``() =
    let result = ref ""
    let throw() : unit =
        raise(exn "Boo!")
    let append(x) = 
        result := !result + x
    let innerAsync() =
        async {
            append "b"
            try append "c"
                throw()
                append "1"
            with _ -> append "d"
            append "e"
        }
    async { 
        append "a"
        try do! innerAsync()
        with _ -> append "2"
        append "f"
    } |> Async.StartImmediate
    equal !result "abcdef"

[<Test>]
let ``Async cancellation works``() =
    async {
        let sleepAndAssign token res =
            Async.StartImmediate(async {
                do! Async.Sleep 100
                res := true
            }, token)
        let res1, res2, res3 = ref false, ref false, ref false
        let tcs1 = new System.Threading.CancellationTokenSource(50)
        let tcs2 = new System.Threading.CancellationTokenSource()
        let tcs3 = new System.Threading.CancellationTokenSource()
        sleepAndAssign tcs1.Token res1
        sleepAndAssign tcs2.Token res2
        sleepAndAssign tcs3.Token res3
        tcs2.Cancel()
        tcs3.CancelAfter(150)
        do! Async.Sleep 125
        equal false !res1
        equal false !res2
        equal true !res3
    } |> Async.RunSynchronously

let successWork: Async<string> = Async.FromContinuations(fun (onSuccess,_,_) -> onSuccess "success")
let errorWork: Async<string> = Async.FromContinuations(fun (_,onError,_) -> onError (exn "error"))
let cancelWork: Async<string> = Async.FromContinuations(fun (_,_,onCancel) ->
        System.OperationCanceledException("cancelled") |> onCancel)

[<Test>]
let ``Async.StartWithContinuations works``() =
    let res1, res2, res3 = ref "", ref "", ref ""
    Async.StartWithContinuations(successWork, (fun x -> res1 := x), ignore, ignore)
    Async.StartWithContinuations(errorWork, ignore, (fun x -> res2 := x.Message), ignore)
    Async.StartWithContinuations(cancelWork, ignore, ignore, (fun x -> res3 := x.Message))
    equal "success" !res1
    equal "error" !res2
    equal "cancelled" !res3

[<Test>]
let ``Async.Catch works``() =
    let assign res = function
        | Choice1Of2 msg -> res := msg
        | Choice2Of2 (ex: Exception) -> res := "ERROR: " + ex.Message
    let res1 = ref ""
    let res2 = ref ""
    async {
        let! x1 = successWork |> Async.Catch
        assign res1 x1
        let! x2 = errorWork |> Async.Catch
        assign res2 x2
    } |> Async.StartImmediate
    equal "success" !res1
    equal "ERROR: error" !res2

[<Test>]
let ``Async.Ignore works``() =
    let res = ref false
    async {
        do! successWork |> Async.Ignore
        res := true
    } |> Async.StartImmediate
    equal true !res

// [<Test>]
// let ``Async.OnCancel works``() =
//     async {
//         let res = ref false
//         let tcs = new System.Threading.CancellationTokenSource()
//         Async.StartImmediate(async {
//             let! _ = Async.OnCancel(fun () -> res := true)
//             do! Async.Sleep 100
//         }, tcs.Token)
//         tcs.CancelAfter 50
//         do! Async.Sleep 75
//         equal true !res
//     } |> Async.RunSynchronously

[<Test>]
let ``Async.Parallel works``() =
    async {
        let makeWork i =
            async {
                do! Async.Sleep 50
                return i
            }
        let res: int[] ref = ref [||]
        let works = [makeWork 1; makeWork 2; makeWork 3]
        async {
            let! x = Async.Parallel works
            res := x
        } |> Async.StartImmediate
        do! Async.Sleep 100
        !res |> Array.sum |> equal 6
    } |> Async.RunSynchronously

open Fable.Core

[<Test>]
let ``Interaction between Async and Promise works``() =
    let res = ref false
    async { res := true }
    #if MOCHA
    |> Async.StartAsPromise
    |> Async.AwaitPromise
    #else
    |> Async.StartAsTask
    |> Async.AwaitTask
    #endif
    |> Async.StartImmediate
    equal true !res
    
[<Test>]
let ``Promises can be cancelled``() =    
    async {
        let res = ref 0
        let tcs = new System.Threading.CancellationTokenSource(50)
        let work =
            let work = async {
                do! Async.Sleep 75
                res := -1
            }
            #if MOCHA
            Async.StartAsPromise(work, tcs.Token) |> Async.AwaitPromise
            #else
            Async.StartAsTask(work, cancellationToken=tcs.Token) |> Async.AwaitTask
            #endif
        Async.StartWithContinuations(work, ignore, ignore, (fun _ -> res := 1))
        do! Async.Sleep 100
        equal 1 !res
    } |> Async.RunSynchronously
