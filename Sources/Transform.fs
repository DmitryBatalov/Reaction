namespace Reaction

open Types
open Core

module Transform =
    // The classic map (select) operator with async mapper
    let mapAsync (mapper : 'a -> Async<'b>) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        let subscribe (aobv : AsyncObserver<'b>) =
            async {
                let _obv n =
                    async {
                        match n with
                        | OnNext x ->
                            let! b =  mapper x
                            do! b |> OnNext |> aobv  // Let exceptions bubble to the top
                        | OnError ex -> do! OnError ex |> aobv
                        | OnCompleted -> do! aobv OnCompleted

                    }
                return! source _obv
            }
        subscribe

    // The classic map (select) operator with sync mapper
    let inline map (mapper : 'a -> 'b) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        mapAsync (fun x -> async { return mapper x }) source

    // The classic map (select) operator with an indexed and async mapper
    let mapiAsync (mapper : 'a*int -> Async<'b>) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        source |> Combine.zipSeq infinite |> mapAsync mapper

    // The classic map (select) operator with sync and indexed mapper
    let inline mapi (mapper : 'a*int -> 'b) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        mapiAsync (fun (x, i) -> async { return mapper (x, i) }) source

    // The classic flap map (selectMany, bind, mapMerge) operator
    let flatMap (mapper : 'a -> AsyncObservable<'b>) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        source |> map mapper |> Combine.merge

    let flatMapi (mapper : 'a*int -> AsyncObservable<'b>) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        source |> mapi mapper |> Combine.merge

    let flatMapAsync (mapper : 'a -> Async<AsyncObservable<'b>>) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        source |> mapAsync mapper |> Combine.merge

    let flatMapiAsync (mapper : 'a*int -> Async<AsyncObservable<'b>>) (source : AsyncObservable<'a>) : AsyncObservable<'b> =
        source |> mapiAsync mapper |> Combine.merge

    let switchLatest (source : AsyncObservable<AsyncObservable<'a>>) : AsyncObservable<'a> =
        let subscribe (aobv : AsyncObserver<'a>) =
            let safeObserver = safeObserver aobv
            let refCount = refCountAgent 1 (async {
                do! safeObserver OnCompleted
            })

            let innerAgent =
                let obv n =
                    async {
                        match n with
                        | OnCompleted -> refCount.Post Decrease
                        | _ -> do! safeObserver n
                    }

                MailboxProcessor.StartImmediate(fun inbox ->
                    let rec messageLoop (current : AsyncDisposable) = async {
                        let! cmd = inbox.Receive()
                        let getCurrent = async {
                            match cmd with
                            | InnerObservable xs ->
                                let! inner = xs obv
                                return inner
                            | Dispose ->
                                do! current ()
                                return disposableEmpty
                        }
                        let! current' = getCurrent
                        return! messageLoop current'
                    }

                    messageLoop disposableEmpty
                )

            async {
                let obv (ns : Notification<AsyncObservable<'a>>) =
                    async {
                        match ns with
                        | OnNext xs ->
                            refCount.Post Increase
                            InnerObservable xs |> innerAgent.Post
                        | OnError e -> do! OnError e |> safeObserver
                        | OnCompleted -> refCount.Post Decrease
                    }

                let! dispose = source obv
                let cancel () =
                    async {
                        do! dispose ()
                        innerAgent.Post Dispose
                    }
                return cancel
            }
        subscribe

    let catch (handler: exn -> AsyncObservable<'a>) (source : AsyncObservable<'a>) : AsyncObservable<'a> =
        let subscribe (aobv : AsyncObserver<'a>) =
            async {
                let mutable disposable = disposableEmpty

                let rec action (source: AsyncObservable<_>) = async {
                    let _obv n = async {
                        match n with
                        | OnError ex ->
                            let nextSource = handler ex
                            do! action nextSource
                        | _ -> do! aobv n
                    }

                    do! disposable ()
                    let! subscription = source _obv
                    disposable <- subscription
                }
                do! action source

                let cancel () =
                    async {
                        do! disposable ()
                    }
                return cancel
            }
        subscribe