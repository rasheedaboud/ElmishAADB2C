# SAFE Template

This template can be used to generate a full-stack web application using the [SAFE Stack](https://safe-stack.github.io/). It was created using the dotnet [SAFE Template](https://safe-stack.github.io/docs/template-overview/). If you want to learn more about the template why not start with the [quick start](https://safe-stack.github.io/docs/quickstart/) guide?

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 5.0 or higher
* [Node LTS](https://nodejs.org/en/download/)

## Starting the application

Before you run the project **for the first time only** you must install dotnet "local tools" with this command:

```bash
dotnet tool restore
```

To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet run
```

Then open `http://localhost:8080` in your browser.

The build project in root directory contains a couple of different build targets. You can specify them after `--` (target name is case-insensitive).

To run concurrently server and client tests in watch mode (you can run this command in parallel to the previous one in new terminal):

```bash
dotnet run -- RunTests
```

Client tests are available under `http://localhost:8081` in your browser and server tests are running in watch mode in console.

Finally, there are `Bundle` and `Azure` targets that you can use to package your app and deploy to Azure, respectively:

```bash
dotnet run -- Bundle
dotnet run -- Azure
```

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)

## Updating Sample
Follow these steps to setup B2C tennant etc. [Docs](https://docs.microsoft.com/en-us/azure/active-directory-b2c/tutorial-create-tenant).  

Navigate to Auth.fs in CLient Project and update AADB2C configuration  

```
let msalConfig ={|
    auth={|
          clientId=""
          authority=""
          knownAuthorities=[|""|]
          redirectUri= "https://localhost:8080/"
          postLogoutRedirectUri = "https://localhost:8080/"|};
    cache={|cacheLocation="sessionStorage"; storeAuthStateInCookie=false|}
  |}
```

Navigate to Server project and update `appsettings.json` 
```
{
    //Reanme the AzureAD if you're using AAD
    "AzureAdB2C": {
        "Instance": "https://<DOMAIN>.b2clogin.com",
        "ClientId": "<API CLIENT ID>",
        "Domain": "<DOMAIN>.onmicrosoft.com",
        "SignUpSignInPolicyId": "", //OMIT IF USING AAD
        "ResetPasswordPolicyId": "", //OMIT IF USING AAD
        "EditProfilePolicyId": "", //OMIT IF USING AAD
        "B2CAppExtentionClientId": "", // If you're using B2C with claims. Leave this out if using AAD
        "TenantId": "",
        "ClientSecret": "" //USE ONLY IF NEEDED
    }
}
```

Follow instrunctions above to start safe app.

Try logging in.

## Supported Actions

For this sample only following methods are available on PublicClientApplication

```
[<Import("PublicClientApplication", from="@azure/msal-browser")>]
type PublicClientApplication (config:obj) =
    abstract member loginRedirect: request:obj -> Promise<unit>;
    abstract member loginPopup: request:obj -> Promise<AuthenticationResult option>
    abstract member logout: unit-> unit
    abstract member getAllAccounts: unit-> account[] 
    abstract member acquireTokenSilent: request:obj -> Promise<AuthenticationResult>;
    abstract member getAccountByUsername:userName: string -> AccountInfo option
```
Extend this as required.

To show or hide UI when user in authenticated or not use either of the following
```
AuthenticatedTemplate.create[
    AuthenticatedTemplate.children[

    ]
]

UnauthenticatedTemplate.create[
    UnauthenticatedTemplate.children[

    ]
] 
```
## Claims

In Auth.fs use type IdTokenClaims to retrive information about response from auth request. Initial model only has minimal claims faimily_name and given_name. Extend this as you see fit.
```
type IdTokenClaims =
  {
    aud: string
    auth_time: string
    emails: string[]
    exp: int
    family_name: string
    given_name: string
    iat: int
    iss: string
    nbf: int
    nonce: string
    sub: string
    tfp: string
    ver: string }
```

## Token
use `aquireTokenSilent(request:TokenRequest)` to try and get JWT Token from B2C. This can be used to make authenticated request to server.

use TokenRequest type to request specific scopes
```
type TokenRequest ={
  account:AccountInfo
  scopes:string[]
  forceRefresh:bool
}
```
AccountInfo can be aquired by calling `getAccountByUsername()` on PublicClientApplication instance.


## Hooks

the following react hook from msal react are supported;  

1. useAccount
2. useIsAuthenticated
3. useMsal
4. useMsalAuthentication
