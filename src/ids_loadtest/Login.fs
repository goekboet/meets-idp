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

type Cred =
    { Id : string
      Email : string
      Password : string
      PasswordHash : string
    }

type Antiforgery =
    { VerificationToken : string
      RequestToken : string
    }

let handler = new HttpClientHandler ( UseCookies = false, AllowAutoRedirect = false )

let httpClient = new HttpClient(handler)

let credsPost 
    (requestToken : string) 
    (creds : Cred) =
    [ new KeyValuePair<string, string>("ReturnUrl", "") 
    ; new KeyValuePair<string, string>("__RequestVerificationToken", requestToken)
    ; new KeyValuePair<string, string>("Email", creds.Email)
    ; new KeyValuePair<string, string>("Password", creds.Password)]


let credsFeed =
    FeedData.fromCsv<Cred> "out-1605969717909.csv"
    |> Feed.createRandom "creds"
    
let getCsrfToken (html : string) : string =
    let doc = HtmlDocument.Parse(html)
    let elt = 
        doc.CssSelect "input[name=__RequestVerificationToken]"
        |> Seq.head
        
    elt.AttributeValue "value"

let getVerificationToken (hs : HttpResponseHeaders) =
    let setCookie = 
        hs.GetValues"Set-Cookie"
        |> Seq.filter (fun x -> x.StartsWith ".AspNetCore.Antiforgery")
        |> Seq.head
        
    setCookie
    |> Seq.takeWhile (fun c -> c <> ';')
    |> Seq.toArray
    |> System.String

let getLogin = Step.create("get login-form", fun ctx -> task {
    let! res = httpClient.GetAsync "https://ids.ego/login"
    let! payload = res.Content.ReadAsStringAsync()
        
    let requestToken = getCsrfToken payload
    let verificationToken = getVerificationToken res.Headers
    let antiforgery = { 
        VerificationToken = verificationToken ; 
        RequestToken = requestToken 
    }

    return Response.Ok(antiforgery)
})

let postLoginMessage 
    (creds : Cred)
    (antiforgery : Antiforgery)
    = 
    let msg = new HttpRequestMessage()
    msg.Method <- System.Net.Http.HttpMethod.Post
    msg.RequestUri <- Uri "https://ids.ego/login/VerifyCredentials"
    msg.Content <- new FormUrlEncodedContent(credsPost antiforgery.RequestToken creds)
    msg.Headers.Add ("Cookie", antiforgery.VerificationToken)

    msg

let postLogin = Step.create("post login-form", credsFeed, fun ctx -> task {
    let antiforgery = ctx.GetPreviousStepResponse<Antiforgery>()
    let msg = postLoginMessage ctx.FeedItem antiforgery
    let! res = httpClient.SendAsync msg

    if (res.StatusCode = System.Net.HttpStatusCode.Redirect)
    then return Response.Ok()
    else return Response.Fail()
})