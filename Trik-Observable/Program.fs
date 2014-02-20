module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq
open Gyroscope
open Config

printfn "First appearance" 

open Extern
open Config
open Sensor3d
open PowerMotor

type GyroInfo = int*int*int

let limit l u (x: int) = Math.Min(u, Math.Max (l, x)) 
let lim1000 = limit -1000 1000
let powerMotors = 
    Array.map (fun (m: Config.DomainTypes.Motor) -> 
            (m.Port, new PowerMotor(Convert.ToInt32(m.I2cCommandNumber, 16) ) )
        ) 
        (config.PowerMotors.GetMotors() )
    |> dict

printfn "Input"

let gyro = new Gyroscope(System.Console.ReadLine() |> Double.Parse)

printfn "gyro created"

let unsub = gyro.Obs.Select(fun (x, y, z) -> (/) 10 <| lim1000  x).Subscribe(powerMotors.[1])
//let unsub = gyro.Obs.Select(fun (x, y, z) -> [| x; y; z; 10 |])  
printfn "subscripted"

System.Console.ReadKey() |> ignore
    
    