module Home

open Elmish
open Fable.Remoting.Client
open Fable.Core
open Shared
open Feliz.UseElmish
open Feliz
open Authorization
open Style
open Feliz.Router

type LoadingState =
    | NotStarted
    | LoadingStarted
    | LoadingFinished
    | LoadingFailed


type State = {
    Todos:Todo list
    LoadingState:LoadingState
    ErrorMessage:string
}
type Msg = 
    | LoadTodos
    | GetToken
    | LoadingStarted of string
    | LoadingFinished of Result<Todo list,RequestErrors>
    | NoTodos
    | ErrorLoadingTodos of string

let requestToken() =   
    client.acquireTokenSilent(tokenRequest)
    |> Promise.map( fun result ->
    match result with
    | Some token -> token.accessToken
    | None -> "") |> Async.AwaitPromise
    

let init ()=

    let model = { 
        Todos =[] 
        LoadingState = NotStarted
        ErrorMessage=""
    }

    model, Cmd.none

let todosApi(token) =
    Remoting.createApi ()
    |> Remoting.withAuthorizationHeader $"Bearer {token}"
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>



let update msg state= 
    match state,msg with
    | _,LoadTodos ->            
        state,Cmd.ofMsg GetToken
    | _, GetToken ->
        {state with LoadingState=LoadingState.LoadingStarted},
        Cmd.OfAsync.either requestToken ()
                        (fun x -> LoadingStarted x)
                        (fun error->ErrorLoadingTodos error.Message)

    | _, NoTodos -> {state with Todos=[]},Cmd.none
    | _, LoadingStarted token ->

        let api = todosApi(token)
        {state with LoadingState=LoadingState.LoadingStarted},
            Cmd.OfAsync.either api.getTodos ()
                            (fun x -> LoadingFinished x)
                            (fun error->ErrorLoadingTodos error.Message)
    | _, LoadingFinished result ->

        match result with
        | Ok todos ->

            {state with Todos=todos; LoadingState=LoadingState.LoadingFinished},Cmd.none
        | Error error ->
            match error with
            | NotAuthorized ->
                state, Cmd.ofMsg (NotAuthorized.value |> ErrorLoadingTodos)
            | ValidationError->
                state, Cmd.ofMsg (ValidationError.value |> ErrorLoadingTodos)
    | _, ErrorLoadingTodos error -> {state with ErrorMessage=error; LoadingState=LoadingState.LoadingFailed},Cmd.none


[<ReactComponent>]
let Home() =
    let state, dispatch = React.useElmish(init, update, [| |])
    Html.div [
        prop.classes [ css.``container-fluid``
                       css.``mt-5``]
        prop.children[
            Html.button [
                prop.classes [  css.btn
                                css.``btn-primary``]
                prop.text "Load Todos"
                prop.onClick (fun _ -> dispatch LoadTodos)
            ]
            match state.LoadingState with
            | LoadingState.NotStarted ->
                Html.div [
                    Html.p [
                        prop.text "Click the button to load the todos!"
                    ]
                ]            
            | LoadingState.LoadingStarted ->
                Html.div [
                    Html.p [
                        prop.text "Loading..."
                    ]
                ]
            | LoadingState.LoadingFinished ->
                Html.div [
                    prop.className css.``mt-3``
                    prop.children [
                        Html.ul[
                            prop.className css.``list-group``
                            prop.children [
                                for todo in state.Todos  do
                                Html.li [
                                    prop.className css.``list-group-item``
                                    prop.text todo.Description
                                ]
                            ]
                        ]
                    ]               
                ]
            | LoadingState.LoadingFailed ->
                Html.div [
                    Html.p [
                        prop.text state.ErrorMessage
                    ]
                ]
        ]
        
    ]


