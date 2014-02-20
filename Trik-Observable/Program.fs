module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq

open Extern
open Config
open Sensor3d
open PowerMotor

let configPathVal = "config.xml"
let config = Config.Load configPathVal

let isWin = (Environment.OSVersion.VersionString = "Microsoft Windows NT 6.2.9200.0")

let path = 
    if isWin then  @"D:\log.txt" 
    else config.Sensors.Gyroscope.DeviceFile

type Gyroscope(rate) =
    let sensor = new Sensor3d(config.Sensors.Gyroscope.Min
                        , config.Sensors.Gyroscope.Max
                        , path, rate)
    do sensor.Start()
    member this.Obs = sensor.Obs

type GyroInfo = int*int*int

let limit l u (x: int) = Math.Min(u, Math.Max (l, x)) 
let lim100 = limit -100 100

let powerMotors = 
    Array.map (fun (m: Config.DomainTypes.Motor) -> 
            (m.Port, new PowerMotor(Convert.ToInt32(m.I2cCommandNumber, 16) ) )
        ) 
        (config.PowerMotors.GetMotors() )
    |> dict

printfn "Input"

let gyro = new Gyroscope(System.Console.ReadLine() |> Double.Parse)

printfn "gyro created"

let unsub = gyro.Obs.Select((fun (x, y, z) -> lim100 x)).Subscribe(powerMotors.[1]) 

printfn "subscripted"

System.Console.ReadKey() |> ignore
    
    