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

[<EntryPoint>]
let main _ = 

    log "Started"
    Helpers.I2C.init "/dev/i2c-2" 0x48 1
    //let model =  Model.Create "config.xml"
    let model = new Model()
    log "Loaded"
    let RightWheel = model.Motor.["JM1"]
    let LeftWheel = model.Motor.["JM2"]
    let wheelSpeed speed = model.AnalogSensor.["JA1"].ToObservable().Scan(fun acc x ->
        printfn "%A" x;
        if x > 35 then 0 else speed).DistinctUntilChanged()

    let r_disp = (wheelSpeed 100).Subscribe(RightWheel)
    let l_disp = (wheelSpeed 70).Subscribe(LeftWheel)
    let gyro_dis = model.Gyro.ToObservable().Subscribe(fun x -> printfn "%A" x)
    log "Ready (any key to finish)"
   
    System.Console.ReadKey() |> ignore
    //l_disp.Dispose()
    //r_disp.Dispose()
    //System.Console.ReadKey() |> ignore


    0
