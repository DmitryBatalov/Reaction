namespace ReAction

open System.Threading
open System

[<AutoOpen>]
module Creation =
    // Create async observable from async worker function
    let fromAsync (worker : AsyncObserver<'a> -> CancellationToken -> Async<unit>) : AsyncObservable<_> =
        let subscribe (aobv : AsyncObserver<_>) : Async<AsyncDisposable> =
            let cancel, token = canceller ()
            let obv = safeObserver aobv

            async {
                let! _ = Async.StartChild (worker obv token, 0)
                return AsyncDisposable cancel
            }
        AsyncObservable subscribe

    // An async observervable that just completes when subscribed.
    let inline empty () : AsyncObservable<'a> =
        fromAsync (fun obv _ -> async {
            do! obv.OnCompleted ()
        })

    // An async observervable that just fails with an error when subscribed.
    let inline fail (exn) : AsyncObservable<'a> =
        fromAsync (fun obv _ -> async {
            do! obv.OnError exn
        })

    let from (xs : seq<'a>) : AsyncObservable<'a> =
        fromAsync (fun obv token -> async {
            for x in xs do
                if token.IsCancellationRequested then
                    raise (OperationCanceledException("Operation cancelled"))

                try
                    do! obv.OnNext x
                with ex ->
                    do! obv.OnError ex

            do! obv.OnCompleted ()
        })

    let inline just (x : 'a) : AsyncObservable<'a> =
        from [ x ]

    // Create an async observable from a subscribe function. So trivial
    // we should remove it once we get used to the idea that subscribe is
    // exactly the same as an async observable.
    let create (subscribe : AsyncObserver<_> -> Async<AsyncDisposable>) : AsyncObservable<_> =
        AsyncObservable subscribe
