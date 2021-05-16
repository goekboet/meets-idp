module Loadtest.Run

open System
open System.Net.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open NBomber.Contracts
open NBomber.FSharp
open NBomber.Plugins.Network.Ping
open Loadtest.Login

[<EntryPoint>]
let main argv =
    // it's optional Ping plugin that brings additional reporting data
    let pingConfig = PingPluginConfig.CreateDefault ["ids.ego"]
    use pingPlugin = new PingPlugin(pingConfig)
    
    // now you use HttpStep instead of NBomber's default Step        
    // wu: 300 d: 900
    Scenario.create "Ids login" [authCodeRequest; getLogin; postLogin; callback; redeemCode; getUserInfo]     
    |> Scenario.withWarmUpDuration(seconds 60)
    |> Scenario.withLoadSimulations [InjectPerSec(rate = 15, during = seconds 60)]
    |> NBomberRunner.registerScenario
    |> NBomberRunner.withWorkerPlugins [pingPlugin]
    |> NBomberRunner.run
    |> ignore

    0 // return an integer exit code