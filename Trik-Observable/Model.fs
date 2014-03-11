namespace Trik
open System
open Trik.ServoMotor

type Model ((*config:Config.Schema.Config*)) = 
    //do printfn "Creating of model"
    do Helpers.Syscall_shell "i2cset -y 2 0x48 0x10 0x1000 w; i2cset -y 2 0x48 0x11 0x1000 w; i2cset -y 2 0x48 0x12 0x1000 w; i2cset -y 2 0x48 0x13 0x1000 w"
    member val Motor = 
        [| 
          ("JM1", 0x14)
          ("JM2", 0x15)
          ("M1", 0x16)
          ("JM3", 0x17)
         |]
        |> Array.map (fun (port, cnum)  ->  (port, new PowerMotor(cnum)))             
        |> dict
         
    member x.Servo = 
        [| 
          ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
            { stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000 } )
          ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
            { stop = 0; zero = 1500000; min = 1200000; max = 1800000; period = 20000000 } )
         |]
        |> Array.map (fun (port, path, kind) ->  ( port, new Servomotor(path, kind)))             
        |> dict
    
    member x.AnalogSensor= 
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

    member x.Gyro = 
                //let c = config.DigitalSensors.Gyroscope 
                new Trik.Gyroscope(-32767, 32767, "/dev/input/by-path/platform-spi_davinci.1-event")//c.Min, c.Max, c.DeviceFile)
    member x.Accel =
                //let c =  config.DigitalSensors.Accelerometer
                new Trik.Accelerometer(-32767, 32767, "/dev/input/event1")//c.Min, c.Max, c.DeviceFile)

    member x.Led = new Trik.Led("/sys/class/leds/")

    member x.Pad = new Trik.PadServer(4444)

    //static member Create(path:string) = new Model(Config.Create path)

    interface IDisposable with
        member x.Dispose() = () // no-no-no...
    

