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
    
    Scenario.create "Ids login" [getLogin; postLogin]     
    |> Scenario.withWarmUpDuration(seconds 5)
    |> Scenario.withLoadSimulations [InjectPerSec(rate = 5, during = seconds 900)]
    |> NBomberRunner.registerScenario
    |> NBomberRunner.withWorkerPlugins [pingPlugin]
    |> NBomberRunner.run
    |> ignore

    0 // return an integer exit code