module Loadtest.Login

open FSharp.Data
open System.Net.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open NBomber.Contracts
open NBomber
open System.Collections.Generic
open System.Net.Http.Headers
open NBomber.FSharp
open FSharp.Control.Tasks.V2.ContextInsensitive
open System
open System.Text.RegularExpressions
open Newtonsoft.Json
open System.Text

type Cred =
    { Id : string
      Email : string
      Password : string
      PasswordHash : string
      SecurityStamp : string
    }

type Antiforgery =
    { VerificationToken : string
      RequestToken : string
      Location : string
      OidcContext : string
      Callback : string
      Sso : string
      Code : string
      AccessToken : string
      RefreshToken : string
    }

let handler = new HttpClientHandler ( UseCookies = false, AllowAutoRedirect = false )

let httpClient = new HttpClient(handler)

let credsPost 
    (returnUrl : string)
    (requestToken : string) 
    (creds : Cred) =
    [ new KeyValuePair<string, string>("ReturnUrl", returnUrl) 
    ; new KeyValuePair<string, string>("__RequestVerificationToken", requestToken)
    ; new KeyValuePair<string, string>("Email", creds.Email)
    ; new KeyValuePair<string, string>("Password", creds.Password)]


let credsFeed =
    FeedData.fromCsv<Cred> "out-1621180524065.csv"
    |> Feed.createRandom "creds"
    
let getCsrfToken (html : string) =
    let doc = HtmlDocument.Parse(html)
    let csrfInput = 
        doc.CssSelect "input[name=__RequestVerificationToken]"
        |> Seq.head

    let returnUrlInput =
        doc.CssSelect "input[name=ReturnUrl]"
        |> Seq.head
        
    (csrfInput.AttributeValue "value", returnUrlInput.AttributeValue "value")


let getVerificationToken (hs : HttpResponseHeaders) =
    let setCookie = 
        hs.GetValues"Set-Cookie"
        |> Seq.filter (fun x -> x.StartsWith ".AspNetCore.Antiforgery")
        |> Seq.head
        
    setCookie
    |> Seq.takeWhile (fun c -> c <> ';')
    |> Seq.toArray
    |> System.String

let getSsoCookie (hs : HttpResponseHeaders)
    =
    let setCookie = 
        hs.GetValues"Set-Cookie"
        |> Seq.filter (fun x -> x.StartsWith "sso")
        |> Seq.head

    setCookie
    |> Seq.takeWhile (fun c -> c <> ';')
    |> Seq.toArray
    |> System.String


let clientId = "dev"
let redirectUrl = @"http://ego.dev"
let codeVerifier = "MHru8CBpeEBRBnaOZoqcedbJK8_Zk03bJ5StlDwTI5E"
let challenge = "ku8vRsgaq3UxbK5wQiR2U3KTZAwmEmRLTD-KlmmxAAU"
let scope = "openid profile"

let authCodeParams
    =
    [ ("client_id", clientId)
    ; ("scope", scope)
    ; ("response_type", "code")
    ; ("redirect_uri", redirectUrl)
    ; ("code_challenge_method", "S256")
    ; ("code_challenge", challenge)
    ]
    |> List.map (fun (k,v) -> sprintf "%s=%s" k v)

let idsLocation = "https://ids.ego"
// let idsLocation = "https://localhost:5001"

let authCodeRequest = Step.create("request auth-code", fun ctx -> task {
    let ps = String.Join("&", authCodeParams) 
    let! r = httpClient.GetAsync(sprintf "%s/connect/authorize?%s" idsLocation ps)
    
    if (r.StatusCode = System.Net.HttpStatusCode.Redirect)
    then 
        let state = { 
            VerificationToken = "" ; 
            RequestToken = "" ;
            Location = r.Headers.Location.ToString();
            OidcContext = "";
            Callback = "";
            Sso = "";
            Code = "";
            AccessToken = "";
            RefreshToken = ""
        }

        return Response.Ok(state)
    else return Response.Fail()
})
    

let getLogin = Step.create("get login-form", fun ctx -> task {
    let authCode = ctx.GetPreviousStepResponse<Antiforgery>()
    let! res = httpClient.GetAsync authCode.Location
    let! payload = res.Content.ReadAsStringAsync()
        
    let (requestToken, returnUrl) = getCsrfToken payload
    let verificationToken = getVerificationToken res.Headers
    
    return Response.Ok(
        { authCode with RequestToken = requestToken; VerificationToken = verificationToken; OidcContext = returnUrl})
})

let postLoginMessage
    (creds : Cred)
    (s : Antiforgery)
    = 
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Post
    msg.RequestUri <- sprintf "%s/login/VerifyCredentials" idsLocation |> Uri
    msg.Content <- new FormUrlEncodedContent(credsPost s.OidcContext s.RequestToken creds)
    msg.Headers.Add ("Cookie", s.VerificationToken)

    msg

let postLogin = Step.create("post login-form", credsFeed, fun ctx -> task {
    let p = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = postLoginMessage ctx.FeedItem p
    let! res = httpClient.SendAsync msg

    if (res.StatusCode = System.Net.HttpStatusCode.Redirect)
    then 
        let sso = getSsoCookie res.Headers
        return Response.Ok({p with Callback = res.Headers.Location.ToString(); Sso = sso })
    else return Response.Fail()
})

let callBackRequest (sso : string) (url : string)
    =
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Get
    msg.RequestUri <- url |> Uri
    msg.Headers.Add ("Cookie", sso)

    msg

let codePattern = @".*code=([^&]+)"
let getCode (l : string)
    =
    let m = Regex.Match(l, codePattern)
    
    m.Groups.[1].Value

let callback = Step.create("authorize callback", fun ctx -> task {
    let p = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = callBackRequest p.Sso (sprintf "%s%s" idsLocation p.Callback)
    let! res = httpClient.SendAsync msg

    if (res.StatusCode = System.Net.HttpStatusCode.Redirect && res.Headers.Location.ToString().StartsWith redirectUrl)
    then
        let code = res.Headers.Location.ToString() |> getCode
        return Response.Ok({p with Code = code })
    else 
        return Response.Fail()
})

let redeemCodeContent (code : string)
    =
    [ new KeyValuePair<string, string>("client_id", clientId) 
    ; new KeyValuePair<string, string>("grant_type", "authorization_code")
    ; new KeyValuePair<string, string>("redirect_uri", redirectUrl)
    ; new KeyValuePair<string, string>("code", code)
    ; new KeyValuePair<string, string>("scope", scope)
    ; new KeyValuePair<string, string>("code_verifier", codeVerifier)]

let redeemCodeRequest (code : string)
    = 
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Post
    msg.RequestUri <- sprintf "%s/connect/token" idsLocation |> Uri
    msg.Content <- new FormUrlEncodedContent(redeemCodeContent code)

    msg

type RedeemCodePayload =
    { [<JsonProperty("access_token")>] AccessToken : string
    ; [<JsonProperty("refresh_token")>] RefreshToken : string
    }

let redeemCode = Step.create("redeem code", fun ctx -> task {
    let p = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = redeemCodeRequest p.Code
    let! res = httpClient.SendAsync msg

    if (res.IsSuccessStatusCode)
    then
        let! json = res.Content.ReadAsStringAsync()
        let auth = JsonConvert.DeserializeObject<RedeemCodePayload>(json)

        return Response.Ok({p with AccessToken = auth.AccessToken; RefreshToken = auth.RefreshToken })
    else 
        return Response.Fail()
})

let getUserInfoRequest (at : string)
    =
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Get
    msg.RequestUri <- sprintf "%s/connect/userinfo" idsLocation |> Uri
    msg.Headers.Authorization <- AuthenticationHeaderValue("Bearer", at)

    msg

let getUserInfo = Step.create("get user-info", fun ctx -> task {
    let p = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = getUserInfoRequest p.AccessToken
    let! res = httpClient.SendAsync msg

    if (res.IsSuccessStatusCode)
    then return Response.Ok(p)
    else return Response.Fail()
})

let refreshTokenContent (rt : string)
    =
    [ new KeyValuePair<string, string>("client_id", clientId)
    ; new KeyValuePair<string, string>("client_secret", clientId) 
    ; new KeyValuePair<string, string>("grant_type", "refresh_token")
    ; new KeyValuePair<string, string>("refresh_token", rt)]

let refreshTokenRequest (rt : string)
    =
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Post
    msg.RequestUri <- sprintf "%s/connect/token" idsLocation |> Uri
    msg.Content <- new FormUrlEncodedContent(refreshTokenContent rt)

    msg

let refreshToken = Step.create("refresh tokens", fun ctx -> task {
    let p = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = refreshTokenRequest p.RefreshToken
    let! res = httpClient.SendAsync msg

    if (res.IsSuccessStatusCode)
    then 
        let! json = res.Content.ReadAsStringAsync()
        let auth = JsonConvert.DeserializeObject<RedeemCodePayload>(json)

        return Response.Ok({p with AccessToken = auth.AccessToken; RefreshToken = auth.RefreshToken })
    else 
        return Response.Fail()
})

let revokePayload (rt : string)
    =
    [ new KeyValuePair<string, string>("token", rt) ]

let revokeTokenMessage (rt : string)
    =
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Post
    msg.RequestUri <- sprintf "%s/connect/revocation" idsLocation |> Uri
    msg.Content <- new FormUrlEncodedContent(revokePayload rt)
    let creds = 
        "dev:dev"
        |> Encoding.ASCII.GetBytes
        |> Convert.ToBase64String

    msg.Headers.Authorization <- AuthenticationHeaderValue("Basic", creds)

    msg

let revokeTokenStep = Step.create("revoke refresh-token", fun ctx -> task {
    let p = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = revokeTokenMessage p.RefreshToken
    let! res = httpClient.SendAsync msg

    if (res.IsSuccessStatusCode)
    then 
        return Response.Ok(p)
    else 
        return Response.Fail()
})