open System
open System.Collections.Generic
open Trik
open System.Threading

let exit = new EventWaitHandle(false, EventResetMode.AutoReset)

[<EntryPoint>]
let main argv = 
    printfn "Started"
    Helpers.I2C.Init "/dev/i2c-2" 0x48 1
    use model = new Model(ServoConfig = [| 
                              ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
                                { stop = 0; zero = 1500000; min = 800000; max = 2400000; period = 20000000 } )
                              ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
                                { stop = 0; zero = 1500000; min = 800000; max = 2400000; period = 20000000 } )
                             |], PadConfigPort = 4444)
   
    let servo1 = model.Servo.["JE2"]
    let servo2 = model.Servo.["JE1"]
    let p1 = model.Motor.["JM2"]
    let p2 = model.Motor.["JM3"]
    //let prX1, prY1, prX2, prY2 = ref -1, ref -1, ref -1, ref -1
    let pad = model.Pad
    use dbtn = pad.Buttons.Subscribe (function 
        | x when x = 5 -> 
            printfn "Exiting (start)"
            exit.Set() |> ignore
        | num -> printfn "%A" num)
    use disp = pad.Pads.Subscribe ( fun (num, coord) ->  
        match num, coord with
        | (1, Some(x, y) ) -> 
            printfn "servo1.SetPower(%d)" x
            servo1.SetPower(x)
            servo2.SetPower(- y * 2)
        | (2, Some(x, y) ) -> 
            printfn "power %d %d" x y
            let base' = y
            let add' = x / 2
            p1.SetPower(base' + ( add') )
            p2.SetPower(base' + (-add') )
        (* | (1, None) -> prX1 := -1; prY1 := -1; printfn "None 1";
        | (2, Some(x, _) ) -> 
            printfn "servo2.SetPower(- %d * 2)" x 
            servo2.SetPower(- x * 2) *)
        | (2, None) -> 
            printfn "None 2" 
            //servo2.Zero();
        | (_, _) -> () 
    )

    printfn "Ready (5 pad btn to finish)"
    exit.WaitOne() |> ignore
    printfn "Exiting (after wait)"
    0
   