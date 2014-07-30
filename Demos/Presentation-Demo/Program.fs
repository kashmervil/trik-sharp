open Trik
open System.Reactive.Linq
[<EntryPoint>]
let main _ = 
    Helpers.I2C.Init "/dev/i2c-2" 0x48 1
    let model = new Model()
    let motorL = model.Motor.["M2"]//Two port in controller's behind
    let motorR = model.Motor.["M1"]//you can change to ports you like 
    //for full ports description and location go http://goo.gl/jRWJ4j
    let power = model.AnalogSensor.["A1"].ToObservable().Select(fun d -> //second port in front 
                            if d > 55 then 100 elif d < 45 then -100 else 0).DistinctUntilChanged()
     
    let l_disp = power.Subscribe(motorL)
    let r_disp = power.Subscribe(motorR)
    let led_disp = power.Select(function | 100 -> LedColor.Green 
                                         | 0   -> LedColor.Orange 
                                         | _   -> LedColor.Red).Subscribe(model.Led)
    System.Console.ReadKey() |> ignore
    0
