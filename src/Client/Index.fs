module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Feliz.UseElmish
open Feliz
open Fable.Core.JsInterop
open Zanaptak.TypedCssClasses
open Authorization
open Browser.Dom


importAll "@fortawesome/fontawesome-free"
importAll "@azure/msal-react"
importAll "@azure/msal-browser"
importAll "@azure/msal-common"

type css = CssClasses<"styles/global.css">
type fa = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/css/all.min.css">



[<RequireQualifiedAccess>]
type Page =
    | Home


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
    | Navigate of Page

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

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
        let! result = client.loginPopup()
        match result with
        | Ok idToken -> return SetUser 
        | Error msg ->
            if forgotPassword msg.errorCode then 
                return ForgotPassword
            else return failwith msg.errorMessage
    } 

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | Navigate page ->
        let model =
            {model with CurrentPage = page; User=getUser() }
        model, Cmd.none
    | LoginError err->
        console.log err
        {model with User=User.Default; IsLoggedIn=false},Cmd.none
    | Login ->       
        model, Cmd.OfPromise.either login () ( fun _ -> SetUser)(fun err -> LoginError err.Message)
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



module Footer = 
    [<ReactComponent>]
    let Footer() =
        
        let copywrite =
            $"""©ElmishAADB2C 2017-{System.DateTime.UtcNow.ToString("yyyy")}"""
        Html.div[
            Html.footer [
                prop.classes [  css.``py-4``
                                css.``bg-light``
                                css.``mt-auto``
                                css.``d-print-none``] 
                prop.children [
                    Html.div [
                        prop.className css.``container-fluid``
                        prop.children [
                            Html.div [
                                prop.classes [css.``d-flex``; css.``justify-content-center``]
                                prop.children[                                    
                                    Html.div [
                                        prop.className css.``text-muted``
                                        prop.text copywrite
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

module Login = 
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
            //prop.className "d-none d-md-flex"
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
module TopBar =
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
                        Login.Login(state,dispatch)
                    ]
                ]
            ]
        ]



[<ReactComponent>]
let App (state: Model,dispatch: Msg -> unit) =

    Html.div [
            prop.className (if state.SideBarOpen then css.``sb-sidenav-toggled``  else css.``sb-nav-fixed``)
            prop.children[
                TopBar.TopBar(state,dispatch)

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
                                        Html.div [
                                            prop.className css.``container-fluid``
                                            prop.children [
                                                // React.router [
                                                //     router.onUrlChanged updateCurrentUrl
                                                //     router.children [
                                                //         match currentUrl with
                                                //         | [ ] -> 
                                                //         | [] -> 
                                                //         | otherwise -> Html.h1 "Not found"
                                                //     ]
                                                // ]                                                                              
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
