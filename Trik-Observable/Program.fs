module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq
open Gyroscope
open Config
open LED

printfn "First appearance" 

open Extern
open Config
open Sensor3d
open PowerMotor

type GyroInfo = int*int*int

linux(fun () -> runInitScript(config))

let limit l u (x: int) = Math.Min(u, Math.Max (l, x)) 
let lim1000 = limit -1000 1000
let powerMotors = 
    Array.map (fun (m: Config.DomainTypes.Motor) -> 
            (m.Port, new PowerMotor(Convert.ToInt32(m.I2cCommandNumber, 16) ) )
        ) 
        (config.PowerMotors.GetMotors() )
    |> dict

printfn "Input"
let led = new LED([| 0x14; 0x15; 0x16; 0x17 |])
let gyro = new Gyroscope(System.Console.ReadLine() |> Double.Parse)
printfn "gyro created"
using (//gyro.Obs.Select(fun (x, y, z) -> (lim1000  x) / 10).Subscribe(powerMotors.[3]) ) (fun unsub ->
    //let unsub = 
    gyro.Obs.Select(fun (x, y, z) -> [| (lim1000  x) / 20 - 50; (lim1000  y) / 20 - 50; (lim1000  x) / 20 - 50; 100 |]).Subscribe(led)) (fun unsub ->
    printfn "subscripted"

    System.Console.ReadKey() |> ignore
    )
    