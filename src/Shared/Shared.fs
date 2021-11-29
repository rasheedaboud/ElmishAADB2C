namespace Shared

open System

type Token = string

type RequestErrors =
    | NotAuthorized
    | ValidationError
    with
        member this.value =
            match this with
            | NotAuthorized -> "Not Authorized"
            | ValidationError -> "Validation Error"

type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos: unit -> Async<Result<Todo list,RequestErrors>>
      addTodo: Todo -> Async<Todo option> }
