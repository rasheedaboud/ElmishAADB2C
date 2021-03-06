module Index

open Elmish
open Fable.Remoting.Client
open Fable.Core
open Shared
open Feliz.UseElmish
open Feliz
open Fable.Core.JsInterop
open Zanaptak.TypedCssClasses
open Authorization
open Browser.Dom
open Feliz.Router


importAll "@fortawesome/fontawesome-free"
importAll "@azure/msal-react"
importAll "@azure/msal-browser"
importAll "@azure/msal-common"

type css = CssClasses<"styles/global.css">
type fa = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/css/all.min.css">



[<RequireQualifiedAccess>]
type Page =
    | Home
    | NotFound


type Model = { 
    CurrentPage: Page;
    SideBarOpen:bool; 
    User:User
    IsLoggedIn:bool
}

type Msg =
    | OpenSideBar
    | CloseSideBar
    | Login
    | SetUser
    | Logout
    | LoginError of string
    | ForgotPassword
    | RouteChanged of string list


let getUser() = 
    client.getAllAccounts()
    |> Array.tryHead 
    |> function
        |Some a -> 
            CreateUser a.idTokenClaims           
        |None   ->  
            CreateUser None




let init () : Model * Cmd<Msg> =

    let accounts =client.getAllAccounts()
    let isLoggedIn = accounts.Length > 0
    let user = getUser()

    let model = { 
        CurrentPage =Page.Home; 
        SideBarOpen=false; 
        User=user
        IsLoggedIn=isLoggedIn
    }

    model, Cmd.none


let logout() =
    client.logout()
    User.Default


let login() =
    promise {
        let! result = client.loginPopup(popupRequest)
        match result with
        | Ok idToken -> return SetUser 
        | Error msg ->
            if forgotPassword msg.errorCode then 
                return ForgotPassword
            else return failwith msg.errorMessage
    } 

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | RouteChanged segments ->
        match segments with
        | [""] -> {model with CurrentPage=Page.Home},Cmd.none
        | _ -> {model with CurrentPage=Page.NotFound},Cmd.none
    | LoginError err->
        console.log err
        {model with CurrentPage=Page.Home; User=User.Default; IsLoggedIn=false},Cmd.none
    | Login ->       
        model, Cmd.OfPromise.either login () 
                                    ( fun _ -> SetUser)
                                    (fun err -> LoginError err.Message)
    | Logout ->        
        {model with User = logout();IsLoggedIn=false},Cmd.none
    | SetUser ->
        {model with User=getUser();IsLoggedIn=true}, Cmd.none
    | ForgotPassword ->
        model,Cmd.none      
    | OpenSideBar ->
        {model with SideBarOpen=true},Cmd.none
    | CloseSideBar ->
        {model with SideBarOpen=false},Cmd.none






[<ReactComponent>]
let LogOutButton(dispatch) =
    Html.button [
        prop.classes [  css.btn
                        css.``btn-outline-light``
                        css.``btn-sm``
                        css.``order-0``
                        css.``ml-2``] 
        prop.onClick( fun _ -> dispatch Logout)                                     
        prop.children [
            Html.span [
                prop.style [style.color.white]
                prop.text "Log Out"                     
            ]                                                        
        ]
    ]
[<ReactComponent>]
let LoginButton(dispatch) =
    Html.button [
        prop.classes [  css.btn
                        css.``btn-outline-light``
                        css.``btn-sm``
                        css.``order-0``
                        css.``ml-2``] 
        prop.onClick( fun _ -> dispatch Login)
        prop.children [              
            Html.span [
                prop.style [style.color.white]
                prop.text "Log In"                                                       
            ]              
        ]
    ]

[<ReactComponent>]
let Login(state,dispatch) =
    Html.div [
        prop.children [
            Html.a [
                prop.style [style.color.white
                            style.margin.auto ]
                prop.href "#"
                prop.onClick(fun x -> 
                        x.preventDefault())
                prop.text state.User.DisplayName
            ]
            if state.IsLoggedIn then
                LogOutButton(dispatch)
            else
                LoginButton(dispatch)    
        ]
    ]


[<ReactComponent>]
let TopBar(state,dispatch) =      

    let drawerClass = 
        if state.SideBarOpen then 
            [fa.fa; fa.``fa-times``] 
        else [fa.fa; fa.``fa-bars``]

    Html.nav [
        prop.classes [  css.``sb-topnav``
                        css.navbar
                        css.``navbar-expand``
                        css.``navbar-dark``
                        css.``bg-dark``] 
        prop.children [            
            AuthenticatedTemplate.create [
                AuthenticatedTemplate.children[
                    Html.button [
                        prop.classes [  css.btn
                                        css.``btn-sm``
                                        css.``btn-outline-light``
                                        css.``order-0``
                                        css.``order-lg-0``
                                        css.``ml-2``] 
                        prop.onClick( fun _->
                                if state.SideBarOpen then 
                                    dispatch CloseSideBar
                                else dispatch OpenSideBar)
                        prop.children [
                            Html.span [
                                prop.style [style.color.white]
                                prop.children [
                                    Html.i [
                                        prop.classes drawerClass                             
                                    ]
                                ]
                            ]                 
                        ]
                    ]
                ]
            ]                         
                
            Html.a [
                prop.classes [css.``navbar-brand``; css.``mt-auto``]
                prop.onClick(fun x -> x.preventDefault())
                prop.text "ELMISH B2C"
            ]
            Html.div [
                prop.className css.``ml-auto``
                prop.children [
                    Login(state,dispatch)
                ]
            ]
        ]
    ]

[<ReactComponent>]
let App (state: Model,dispatch: Msg -> unit) =
    Html.div [
            prop.className (if state.SideBarOpen then css.``sb-sidenav-toggled``  else css.``sb-nav-fixed``)
            prop.children[
                TopBar(state,dispatch)

                Html.div[
                    prop.id "layoutSidenav"
                    prop.children [
                        Html.div [
                            prop.id "layoutSidenav_nav"
                            prop.children [
                                Html.nav [
                                    prop.id "sidenavAccordion"
                                    prop.classes [  css.``sb-sidenav``
                                                    css.accordion
                                                    css.``sb-sidenav-dark``] 
                                    prop.children [
                                        Html.div [
                                            prop.className css.``sb-sidenav-menu``
                                            prop.children [
                                                Html.div [
                                                    prop.className css.nav
                                                    prop.children [
                                                        //NavLinks()
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    
                        Html.div [ 
                            prop.id "layoutSidenav_content"
                            prop.children [
                                Html.main [
                                    prop.children [
                                        AuthenticatedTemplate.create [
                                            AuthenticatedTemplate.children [
                                                Html.div [
                                                    prop.className css.``container-fluid``
                                                    prop.children [
                                                        React.router [
                                                            router.onUrlChanged (RouteChanged >> dispatch)
                                                            router.children [
                                                                match state.CurrentPage with
                                                                | Page.Home -> Home.Home()
                                                                //| [""] -> Home.Home()
                                                                | otherwise -> Html.h1 "Not found"
                                                            ]
                                                        ]                                                                              
                                                    ]
                                                ]
                                            ]

                                        ]
                                        UnauthenticatedTemplate.create [
                                            UnauthenticatedTemplate.children[
                                                Html.div [
                                                    Html.p [
                                                        prop.classes [css.h2; css.``justify-content-center``]
                                                        prop.text "Login to get started."
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]                        
                                ]
                                Footer.Footer()
                            ]
                        ]
                    ]               
                ]          
            ]
        ]

let view (state: Model) (dispatch: Msg -> unit) =
    MsalProvider.create[
        MsalProvider.instance client
        MsalProvider.children[
            App(state, dispatch)
        ]
    ]
