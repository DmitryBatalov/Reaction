module Tests.Utils

open System
open System.Collections.Generic
open System.Threading.Tasks

open ReAction

type TestObserver<'a> () =
    let notifications = new List<Notification<'a>>()
    let completed = TaskCompletionSource<'a>()
    let monitor = new Object ()

    let mutable latest : 'a option = None

    member this.Notifications = notifications

    member this.Callable =
        let obv (n : Notification<'a>) = async {
            printfn "TestObserver %A" n

            lock monitor (fun () ->
                this.Notifications.Add(n)
            )

            match n with
            | OnNext x -> latest <- Some x
            | OnError e -> completed.SetException e
            | OnCompleted ->
                match latest with
                | Some x -> completed.SetResult x
                | None -> completed.SetCanceled ()
        }
        AsyncObserver obv

    member this.Await () : Async<'a> =
        async {
            return! Async.AwaitTask completed.Task
        }

    member this.AwaitIgnore () : Async<unit> =
        async {
            try
                do! Async.AwaitTask completed.Task |> Async.Ignore
            with
            | :? TaskCanceledException -> ()
        }

let fromNotification (notifications : seq<Notification<'a>>) =
    fromAsync (fun obv token -> async {
        for notification in notifications do
            if token.IsCancellationRequested then
                raise (OperationCanceledException("Operation cancelled"))

            try
                do! obv.Call notification
            with ex ->
                do! obv.OnError ex
    })

