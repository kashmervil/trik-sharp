open System
open System.Collections.Generic
open Trik

[<EntryPoint>]
let main argv = 
    printfn "Started"
    Helpers.I2C.init "/dev/i2c-2" 0x48 1
    use model = new Model(ServoConfig = [| 
                              ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
                                { stop = 0; zero = 1310000; min = 1200000; max = 1420000; period = 20000000 } )
                              ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
                                { stop = 0; zero = 1550000; min =  800000; max = 2250000; period = 20000000 } )
                             |], PadConfigPort = 4444)
   
    //let servo1 = model.Servo.["JE2"]
    //let servo2 = model.Servo.["JE1"]
    let prX1, prY1, prX2, prY2 = ref -1, ref -1, ref -1, ref -1
    let pad = model.Pad
    use dbtn = pad.Buttons.Subscribe (fun num ->  printfn "%A" num)
    use disp = pad.Pads.Subscribe ( fun (num, coord) ->  
        match num, coord with
        | (1, Some(x, y) ) -> 
            printfn "servo1.SetPower(%d)" x //servo1.SetPower(x)
        | (1, None) -> prX1 := -1; prY1 := -1; printfn "None 1";
        | (2, Some(x, _) ) -> printfn "servo2.SetPower(- %d * 2)" x //servo2.SetPower(- x * 2)
        | (2, None) -> printfn "None 2" //servo2.Zero();
        | (_, _) -> () 
    )

    printfn "Ready (any key to finish)"
    System.Console.ReadKey() |> ignore
    0
   