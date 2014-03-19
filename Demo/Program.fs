module Program

open System
open System.Collections.Generic
open System.Reactive.Linq
open Trik

type Distance =  Far | Middle | Near

let log s = printfn s

let testPad (model:Model) = 
    use servo1 = model.Servo.["JE2"]
    use servo2 = model.Servo.["JE1"]
    let prX1, prY1, prX2, prY2 = ref -1, ref -1, ref -1, ref -1
    use pad = model.Pad
    use dbtn = pad.Buttons.Subscribe (fun num ->  printfn "%A" num)
    use disp = pad.Pads.Subscribe ( fun (num, coord) ->  
        //printfn "%A %A" num coord 
        match num, coord with
        | (1, Some(x, y) ) -> 
            if !prX1 = -1 then prX1 := x; prY1 := y 
            else servo1.SetPower(x - !prX1)
        | (1, None) -> prX1 := -1; prY1 := -1
        | (2, Some(x, _) ) -> servo2.SetPower(- x * 2)
        | (2, None) -> servo2.Zero()
        | (_, _) -> () 
    )

    log "Ready (any key to finish)"
    System.Console.ReadKey() |> ignore
   
let testMain (model:Model) = 
    let rawToDist x = match x with 
                      | _ when x > 80 -> Near
                      | _ when x > 30 -> Middle
                      | _ -> Far
    let distToSpeed x = match x with
                        | Near -> -100
                        | Middle -> 0
                        | Far -> 100

    use rightWheel = model.Motor.["JM1"]
    use leftWheel = model.Motor.["JM2"]
    let frontSensor = model.AnalogSensor.["JA1"].ToObservable().Select(rawToDist)
    let motorActions = frontSensor.Select(fun x -> // printfn "%A" x;
                                                      distToSpeed x).DistinctUntilChanged()

    use r_disp = motorActions.Select(fun x -> -x).Subscribe(rightWheel)
    use l_disp = motorActions.Subscribe(leftWheel)
    //let gyro_dis = model.Gyro.ToObservable().Subscribe(fun x -> printfn "%A" x)
    log "Ready (any key to finish)"
    System.Console.ReadKey() |> ignore

[<EntryPoint>]
let main _ = 
    log "Started"
    Helpers.I2C.init "/dev/i2c-2" 0x48 1
    use model = new Model(ServoConfig = [| 
                              ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
                                { stop = 0; zero = 1310000; min = 1200000; max = 1420000; period = 20000000 } )
                              ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
                                { stop = 0; zero = 1550000; min =  800000; max = 2250000; period = 20000000 } )
                             |])
    log "Loaded"
    testPad model
    //testMain(model)
    0
