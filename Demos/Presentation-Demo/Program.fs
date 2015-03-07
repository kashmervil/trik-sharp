open Trik
open Trik.Collections
open System.Reactive.Linq
[<EntryPoint>]
let main _ = 
    let model = new Model()
    let motorL = model.Motors.[M2]//Two ports in the controller's behind
    let motorR = model.Motors.[M1]//you can change ports to any of the range M1 .. M4 
    //for full ports description go http://goo.gl/jRWJ4j
    let power = model.AnalogSensors.[A1].ToObservable().Select(fun d -> //second port in front 
                            if d < 450 then 100 elif d > 550 then -100 else 0).DistinctUntilChanged()
     
    let l_disp = power.Subscribe(motorL)
    let r_disp = power.Subscribe(motorR)
    let led_disp = power.Select(function | 100 -> LedColor.Green 
                                         | 0   -> LedColor.Orange 
                                         | _   -> LedColor.Red).Subscribe(model.Led)
    System.Console.ReadKey() |> ignore
    0
