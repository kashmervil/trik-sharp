module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq
open Trik
open Trik.Observable

type GyroInfo = int*int*int
[<EntryPoint>]
let main _ = 
    let config =  Config.Create "config.xml"
    let inline limit1000 v = Helpers.limit -1000 1000 v

    printfn "Input"
    let led = new LED([| 0x14; 0x15; 0x16; 0x17 |])
    let gyro = new Gyroscope(config)
    printfn "gyro created"
    use s = gyro.Obs.Select(fun (x, y, z) -> [| (limit1000  x) / 20 - 50; (limit1000  y) / 20 - 50; (limit1000  x) / 20 - 50; 100 |]).Subscribe(led)
    printfn "subscribed"
    System.Console.ReadKey() |> ignore
    0
    
    