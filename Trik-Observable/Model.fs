namespace Trik
open System
open Trik.Observable

type Model (config:Config.Schema.Config) = 
    let powerMotors  = 
        Array.map (fun (m: Config.Schema.Motor) -> 
                (m.Port, new PowerMotor(Convert.ToInt32(m.I2cCommandNumber, 16) ) )
            ) 
            (config.PowerMotors.GetMotors() )
        |> dict
    member x.Motors = powerMotors
    member val Gyro = new Trik.Observable.Gyroscope(config)
    

