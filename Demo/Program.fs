module Program

open System
open System.Collections.Generic
open System.Reactive.Linq
open Trik


let lpf (avg:IList<'a>->'a) (o:IObservable<_>) = o.Buffer(5).Select(avg)

//let avg3 (buf:IList<_>) = 
//        let (x',y',z') = buf.Aggregate(fun (x,y,z) (x',y', z') -> (x + x', y + y', z+ z'))
//        x'/buf.Count, y'/buf.Count, z'/buf.Count 
let log s = printfn "%s" s

type Distance =  Far | Middle | Near

[<EntryPoint>]
let main _ = 

    log "Started"
    Helpers.I2C.init "/dev/i2c-2" 0x48 1
    let model = new Model()
    log "Loaded"

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
    eprintfn "%A" <| l_disp.GetType().Name
    System.Console.ReadKey() |> ignore


    0
