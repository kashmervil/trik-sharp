namespace Trik
open System
open System.Collections.Generic

type Model () as model = 

    static do Helpers.trikSpecific <| fun () -> Helpers.I2C.Init "/dev/i2c-2" 0x48 1
                                                IO.File.WriteAllText("/sys/class/gpio/gpio62/value", "1")
    static let resources = new ResizeArray<_>()

    let mutable gyro = None
    let mutable accel = None
    let mutable led = None
    let mutable pad = None
    let mutable motor = None
    let mutable ledStripe = None
    let mutable servo = None
    let mutable encoder = lazy (
        model.EncoderConfig 
        |> Array.map (fun (port, cnum) -> (port, new Encoder(cnum)))
        |> dict
    )
    let mutable analogSensor = None
    let mutable isDisposed = false
    do AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> (model :> IDisposable).Dispose())

    member val PadConfigPort = 3333 with get, set
    member val ServoConfig = 
        [| 
          ("E1", "/sys/class/pwm/ehrpwm.1:1", 
            { stop = 0; zero = 1450000; min = 1200000; max = 1800000; period = 20000000 } )
          ("E2", "/sys/class/pwm/ehrpwm.1:0", 
            { stop = 0; zero = 1450000; min = 1200000; max = 1800000; period = 20000000 } )
         |] with get, set
    member val EncoderConfig =
        [| 
          ("B2", 0x31)
          ("B4", 0x32)
          ("B3", 0x33)
         |] with get, set
    member val MotorConfig = 
        [| 
          ("M1", 0x14)
          ("M2", 0x15)
          ("M3", 0x16)
          ("M4", 0x17)
         |] with get, set
    member val LedStripeConfig = {Red = 0x14; Green = 0x15; Blue = 0x16; Ground = 0x17;}
         with get, set
    member val AnalogSensorConfig = 
        [| 
          ("A1", 0x25)
          ("A2", 0x24)
          ("A3", 0x23)
          ("A4", 0x22)
          ("A5", 0x21)
          ("A6", 0x20)
        |] with get, set
    member x.Motor
        with get() = 
            let motorInit() = 
                [0x10 .. 0x13] |>  List.iter (fun x -> Helpers.I2C.Send x 0x1000 2)
                motor <- 
                    x.MotorConfig
                    |> Array.map (fun (port, cnum)  -> (port, new PowerMotor(cnum)))             
                    |> dict
                    |> Some
            if motor.IsNone then motorInit() 
            motor.Value
 
         
    member x.Servo
        with get() = 
            let servoInit() = 
                servo <- 
                    x.ServoConfig
                    |> Array.map (fun (port, path, kind) ->  (port, new ServoMotor(path, kind)))             
                    |> dict
                    |> Some
            if servo.IsNone then servoInit() 
            servo.Value
        
    
    member x.AnalogSensor
        with get() = 
            let analogSensorDefaultInit() = 
                analogSensor <-
                    x.AnalogSensorConfig
                    |> Array.map (fun (port, cnum) -> (port, new AnalogSensor(cnum)))
                    |> dict
                    |> Some
            if analogSensor.IsNone then analogSensorDefaultInit()
            analogSensor.Value
    
    member x.Encoder
        with get() = encoder.Force()

    member x.Gyro
        with get() = 
            let gyroDefaultInit() =
                gyro <- Some(new Trik.Gyroscope(-32767, 32767, "/dev/input/by-path/platform-spi_davinci.1-event"))
            if gyro.IsNone then gyroDefaultInit()
            gyro.Value

    member x.Accel
        with get() = 
            let accelDefaultInit() = 
                accel <- Some(new Trik.Accelerometer(-32767, 32767, "/dev/input/event1"))
            if accel.IsNone then accelDefaultInit()
            accel.Value
        
    member x.Led 
        with get() = 
            let ledDefaultInit() =
                led <- Some(new Trik.Led("/sys/class/leds/"))
            if led.IsNone then ledDefaultInit()
            led.Value
    member x.LedStripe
        with get() = 
            if ledStripe.IsNone then
                [0x10 .. 0x13] |>  List.iter (fun x -> Helpers.I2C.Send x 0x1000 2)
                ledStripe <- Some(new Trik.LedStripe(x.LedStripeConfig))
            ledStripe.Value
    
    member x.Pad 
        with get() = 
            let padDefaultInit() =
                pad <- Some(new Trik.PadServer(x.PadConfigPort))
            if pad.IsNone then padDefaultInit()
            pad.Value

    static member RegisterResource(d: IDisposable) = lock resources <| fun () -> resources.Add(d)

    interface IDisposable with
        member self.Dispose() = 
            lock self 
            <| fun () -> 
                   if not isDisposed then
                        resources.ForEach(fun x -> x.Dispose()) 
                        let inline dispose (device: 'T option when 'T :> IDisposable) = 
                             device |> Option.iter (fun x -> x.Dispose())
                        let inline disposeMap (devices: IDictionary<string, 'T> option when 'T :> IDisposable) = 
                            devices |> Option.iter (Seq.iter (fun x -> x.Value.Dispose()))
                        dispose gyro
                        dispose accel
                        dispose led
                        dispose pad
                        disposeMap motor
                        disposeMap servo
                        disposeMap analogSensor
                        dispose ledStripe
                        isDisposed <- true
            