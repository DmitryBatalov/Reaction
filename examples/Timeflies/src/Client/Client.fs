module Client

open Fable.Helpers.React
open Fable.Helpers.React.Props

open ReAction
open Fable.ReAction

open Fable.Import.React
open Fable

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = {
    Letters: Map<int, string * int * int>
}

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | Letter of int * string * int * int

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (currentModel : Model) (msg : Msg) : Model =
    match currentModel.Letters, msg with
    | _, Letter (i, c, x, y) ->
        { currentModel with Letters = currentModel.Letters.Add (i, (c, x, y)) }

let view (model : Model) =
    let letters = model.Letters
    let offsetX x i = x + i * 10 + 15

    div [ Style [ FontFamily "Consolas, monospace"]] [
        for KeyValue(i, (c, x, y)) in letters do
            yield span [ Style [Top y; Left (offsetX x i); Position "absolute"] ] [
                str c
            ]
    ]

let main = async {
    let initialModel = { Letters = Map.empty }

    let moves =
        Seq.toList "TIME FLIES LIKE AN ARROW" |> Seq.map string |> from
            |> flatMapi (fun (x, i) ->
                fromMouseMoves ()
                    |> map (fun m -> Letter (i, x, int m.clientX, int m.clientY))
                    |> delay (100 * i)
            )
            |> scan initialModel update
            |> map view

    let obv = renderReact "elmish-app"
    do! moves.SubscribeAsyncIgnore obv
}

main |> Async.StartImmediate
