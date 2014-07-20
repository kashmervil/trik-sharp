open Trik
[<EntryPoint>]
let main _ = 
    use model = new Model()
    let motorL = model.Motor.["JM2"]//Two port in controller's behind
    let motorR = model.Motor.["JM1"]//you can change to ports you like 
    //for full ports description and location go http://goo.gl/jRWJ4j
    let power = model.AnalogSensor.["JA1"].ToObservable() 
                |> Observable.map (fun d -> if d > 55 then 100 elif d < 45 then -100 else 0) 
                |> Observable.DistinctUntilChanged

    use l_disp = power.Subscribe(motorL)
    use r_disp = power.Subscribe(motorR)
    let ledStream = power |> Observable.map (function | 100 -> LedColor.Green 
                                                      | 0   -> LedColor.Orange 
                                                      | _   -> LedColor.Red)
    use led_disp = ledStream.Subscribe(model.Led)
    
    System.Console.ReadKey() |> ignore
    0
