open Trik
open System.Reactive.Linq
[<EntryPoint>]
let main _ = 
    Helpers.I2C.Init "/dev/i2c-2" 0x48 1
    use model = new Model()
    let motorL = model.Motor.["JM2"]//Two port in controller's behind
    let motorR = model.Motor.["JM1"]//you can change to ports you like 
    //for full ports description and location go http://goo.gl/jRWJ4j
    let power = model.AnalogSensor.["JA1"].ToObservable().Select(fun d -> //second port in front 
                            if d > 55 then 100 elif d < 45 then -100 else 0).DistinctUntilChanged()
     
    use l_disp = power.Subscribe(motorL)
    use r_disp = power.Subscribe(motorR)
    use led_disp = power.Select(function | 100 -> LedColor.Green 
                                         | 0   -> LedColor.Orange 
                                         | _   -> LedColor.Red).Subscribe(model.Led)
    System.Console.ReadKey() |> ignore
    0
