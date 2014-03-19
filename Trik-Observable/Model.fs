namespace Trik
open System
open Trik.ServoMotor

type Model () = 
    //do printfn "Creating of model"
    do Helpers.I2C.send 0x10 0x1000 2
    do Helpers.I2C.send 0x11 0x1000 2
    do Helpers.I2C.send 0x12 0x1000 2
    do Helpers.I2C.send 0x13 0x1000 2
    //Helpers.Syscall_shell "i2cset -y 2 0x48 0x10 0x1000 w; i2cset -y 2 0x48 0x11 0x1000 w; i2cset -y 2 0x48 0x12 0x1000 w; i2cset -y 2 0x48 0x13 0x1000 w"
    
    let mutable gyro = None
    let mutable accel = None
    let mutable led = None
    let mutable pad = None
    let mutable motor = None
    let mutable servo = None
    let mutable analogSensor = None

    member val ServoConfig = 
        [| 
          ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
            { stop = 0; zero = 1450000; min = 1200000; max = 1800000; period = 20000000 } )
          ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
            { stop = 0; zero = 1450000; min = 1200000; max = 1800000; period = 20000000 } )
         |] with get, set
    member val MotorConfig = 
        [| 
          ("JM1", 0x14)
          ("JM2", 0x15)
          ("M1", 0x16)
          ("JM3", 0x17)
         |] with get, set
    member val AnalogSensorConfig = 
        [| 
          ("JA1", 0x25 )
          ("JA2", 0x24 )
          ("JA3", 0x23 )
          ("JA4", 0x22 )
          ("JA5", 0x21 )
          ("JA6", 0x20 )
        |]
    member x.Motor
        with get() = 
            match motor with 
            | None -> 
                motor <- 
                    x.MotorConfig
                    |> Array.map (fun (port, cnum)  -> (port, new PowerMotor(cnum)))             
                    |> dict
                    |> Some
                motor.Value
            | Some(x) -> x
         
    member x.Servo
        with get() = 
            match servo with 
            | None -> 
                servo <- 
                    x.ServoConfig
                    |> Array.map (fun (port, path, kind) ->  ( port, new Servomotor(path, kind)))             
                    |> dict
                    |> Some
                servo.Value
            | Some(x) -> x
        
    
    member x.AnalogSensor
        with get() = 
            match analogSensor with 
            | None -> 
                analogSensor <-
                    x.AnalogSensorConfig
                    |> Array.map (fun (port, cnum) -> (port, new AnalogSensor(cnum)))
                    |> dict
                    |> Some
                analogSensor.Value
            | Some(x) -> x

    member x.Gyro
        with get() = 
            match gyro with 
            | None -> 
                gyro <- Some(new Trik.Gyroscope(-32767, 32767, "/dev/input/by-path/platform-spi_davinci.1-event"))
                gyro.Value
            | Some(x) -> x

    member x.Accel
        with get() = 
            match accel with 
            | None -> 
                accel <- Some(new Trik.Accelerometer(-32767, 32767, "/dev/input/event1"))
                accel.Value
            | Some(x) -> x
        
    member x.Led 
        with get() = 
            match led with 
            | None -> 
                led <- Some(new Trik.Led("/sys/class/leds/"))
                led.Value
            | Some(x) -> x

    member x.Pad 
        with get() = 
            match pad with 
            | None -> 
                pad <- Some(new Trik.PadServer(4444))
                pad.Value
            | Some(x) -> x

    interface IDisposable with
        member x.Dispose() = 
            match motor with 
            | None -> ()
            | Some(motor) -> motor |> Seq.iter (fun x -> (x.Value :> IDisposable).Dispose() )
            
    

