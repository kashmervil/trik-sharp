module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq
open Trik
open Trik.Observable

let inline limit1000 v = Helpers.limit -1000 1000 v
    

[<EntryPoint>]
let main _ = 
    let model =  Model.Create "config.xml"
    let leds = new LedStripe([| 0x14; 0x15; 0x16; 0x17 |])

    //use s = model.Accel.Observable. Select(fun (x,y,z)  .Subscribe(leds)
    printfn "subscribed"
    System.Console.ReadKey() |> ignore
    0
    
    