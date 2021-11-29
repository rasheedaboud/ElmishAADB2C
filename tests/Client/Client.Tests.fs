module Client.Tests

open Fable.Mocha

open Index
open Shared

let client = testList "Client" [
    testCase "Open sidebar" <| fun _ ->
        let newTodo = Todo.create "new todo"
        let model, _ = Index.init ()

        let model, _ = Index.update (Index.Msg.OpenSideBar) model

        Expect.equal true model.SideBarOpen "SideBar shoudl be open"
]

let all =
    testList "All"
        [
#if FABLE_COMPILER // This preprocessor directive makes editor happy
            Shared.Tests.shared
#endif
            client
        ]

[<EntryPoint>]
let main _ = Mocha.runTests all