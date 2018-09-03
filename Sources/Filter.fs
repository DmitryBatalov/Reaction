namespace Reaction

open Types
open Core

module Filter =
    let choose (chooser: 'a -> 'b option) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        let subscribe (obvAsync : Types.AsyncObserver<'b>) =
            async {
                let _obv n =
                    async {
                        match n with
                        | OnNext x ->
                            // Let exceptions bubble to the top
                            match chooser x with
                            | Some b ->
                                do! OnNext b |> obvAsync
                            | None -> ()
                        | OnError ex -> do! OnError ex |> obvAsync
                        | OnCompleted -> do! OnCompleted |> obvAsync

                    }
                return! _obv |>source
            }
        subscribe

    // The classic filter (where) operator with async predicate
    let filterAsync (predicate : 'a -> Async<bool>) (source : AsyncObservable<'a>) : AsyncObservable<'a> =
        let subscribe (aobv : AsyncObserver<'a>) =
            async {
                let obv n =
                    async {
                        match n with
                        | OnNext x ->
                            let! result = predicate x
                            if result then
                                do! x |> OnNext |> aobv  // Let exceptions bubble to the top
                        | _ -> do! aobv n
                    }
                return! source obv
            }
        subscribe

    let distinctUntilChanged (source : AsyncObservable<'a>) : AsyncObservable<'a> =
        let subscribe (aobv : AsyncObserver<'a>) =
            let safeObserver = safeObserver aobv
            let agent = MailboxProcessor.Start(fun inbox ->
                let rec messageLoop (latest : Notification<'a>) = async {
                    let! n = inbox.Receive()

                    let! latest' = async {
                        match n with
                        | OnNext x ->
                            if n <> latest then
                                try
                                    do! OnNext x |> safeObserver
                                with
                                | ex -> do! OnError ex |> safeObserver
                        | _ ->
                            do! safeObserver n
                        return n
                    }

                    return! messageLoop latest'
                }

                messageLoop OnCompleted // Use as sentinel value as it will not match any OnNext value
            )

            async {
                let obv n =
                    async {
                        agent.Post n
                    }
                return! source obv
            }
        subscribe

