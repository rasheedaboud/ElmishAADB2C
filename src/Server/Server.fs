module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Identity.Web
open Shared
open Microsoft.Extensions.Logging
open Giraffe.HttpStatusCodeHandlers

type Storage() =
    let todos = ResizeArray<_>()

    member __.GetTodos() = List.ofSeq todos

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok()
        else
            Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project")
|> ignore

storage.AddTodo(Todo.create "Write your app")
|> ignore

storage.AddTodo(Todo.create "Ship it !!!")
|> ignore



let todosApi (ctx: HttpContext): ITodosApi = {

    getTodos =
        fun () ->
            
            if ctx.User.Identity.IsAuthenticated then
                storage.GetTodos() |> Ok |> async.Return
            else
                Error Shared.RequestErrors.NotAuthorized |> async.Return

    addTodo =
        fun todo ->
            if ctx.User.Identity.IsAuthenticated then
                
                async {
                    match storage.AddTodo todo with
                    | Ok () -> return Some todo
                    | Error e -> return failwith e
                }
            else
                None |> async.Return
}

let errorHandler (ex:exn) (ctx:RouteInfo<HttpContext>) =
    let logger = ctx.httpContext.GetService<ILogger<ITodosApi>>()
    logger.LogError(ex, $"Error occurred in method '{ctx.methodName}' for route '{ctx.path}'. Request Body: '{ctx.requestBodyText}'.")
    Propagate ex.Message

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.withErrorHandler errorHandler
    |> Remoting.fromContext todosApi
    |> Remoting.buildHttpHandler


type Startup(configuration: IConfiguration) =
    
    member _.Configuration = configuration

    member _.ConfigureServices(services: IServiceCollection) =

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAdB2C")) |> ignore
        services.AddGiraffe() |> ignore

    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        app
           .UseAuthentication()
           .UseAuthorization()
           .UseGiraffe webApp


let CreateHostBuilder args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder.UseStartup<Startup>()
                      .ConfigureAppConfiguration(fun context builder ->
                        builder.AddJsonFile("appsettings.json", true, true)|>ignore) |>ignore
        )

[<EntryPoint>]
let main args =
    CreateHostBuilder(args).Build().Run()

    0
