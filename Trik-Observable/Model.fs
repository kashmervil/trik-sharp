namespace Trik
open System
open Trik.ServoMotor

type Model (config:Config.Schema.Config) = 
    member val Motor = 
        [| 
          ("JM1", 0x14)
          ("JM2", 0x15)
          ("M1", 0x16)
          ("JM3", 0x17)
         |]
        |> Array.map (fun (port, cnum)  ->  (port, new PowerMotor(cnum)))             
        |> dict
         
    member val Servo = 
        [| 
          ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
            { stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000 } )
          ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
            { stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000 } )
         |]
        |> Array.map (fun (port, path, kind) ->  ( port, new Servomotor(path, kind)))             
        |> dict
    
    member val AnalogSensor= 
        [| 
          ("JA1", 0x25 )
          ("JA2", 0x24 )
          ("JA3", 0x23 )
          ("JA4", 0x22 )
          ("JA5", 0x21 )
          ("JA6", 0x20 )
        |]
        |> Array.map (fun (port, cnum) -> (port, new AnalogSensor(cnum)))
        |> dict

    member val Gyro = 
                let c = config.DigitalSensors.Gyroscope 
                new Trik.Gyroscope(c.Min, c.Max, c.DeviceFile)
    member val Accel =
                let c =  config.DigitalSensors.Accelerometer
                new Trik.Accelerometer(c.Min, c.Max, c.DeviceFile)
    member val Led = new Trik.Led("/sys/class/leds/")

    member val Pad = new Trik.PadServer(4444)

    static member Create(path:string) = new Model(Config.Create path)

    interface IDisposable with
        member x.Dispose() = () // no-no-no...
    

