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
    let led = new LED([| 0x14; 0x15; 0x16; 0x17 |])
    use s = model.Gyro.Obs.Select(fun (x, y, z) -> [| (limit1000  x) / 20 - 50; (limit1000  y) / 20 - 50; (limit1000  x) / 20 - 50; 100 |]).Subscribe(led)
    printfn "subscribed"
    System.Console.ReadKey() |> ignore
    0
    
    