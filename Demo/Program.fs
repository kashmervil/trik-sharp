module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq
open System.Reactive.Joins
open System.Linq
open Trik


let lpf (avg:IList<'a>->'a) (o:IObservable<_>) = o.Buffer(5).Select(avg)

let avg3 (buf:IList<_>) = 
        let (x',y',z') = buf.Aggregate(fun (x,y,z) (x',y', z') -> (x + x', y + y', z+ z'))
        x'/buf.Count, y'/buf.Count, z'/buf.Count  
        
let log s = printfn "%s" s

[<EntryPoint>]
let main _ = 

    log "Started"
    let model =  Model.Create "config.xml"
    log "Loaded"
    
    // Actuators/Observers
    let leds = new LedStripe(0x14, 0x15, 0x16, 0x17)
    //let arm = model.Servo.["JE1"] 
    //let hand =  model.Servo.["JE2"]
        
    // Sensors/Observables
    //let accel = model.Accel.ToObservable |> lpf avg3
    //let gyro = model.Gyro.ToObservable |> lpf avg3
    
    

    let distance = model.AnalogSensor.["JA1"].Observable |> lpf (fun buf -> int <| buf.Average()) 
    //use h = distance.Skip(10).Subscribe(fun x -> printfn "%d" x)

    (*
    let trafficLight ((x, y, z) as arg) =
        if x < z then LedColor.Red 
        elif  x < y  then LedColor.Orange 
        else LedColor.Green  
    
    use h = accel.Select(trafficLight).Subscribe(model.Led)
    
    let accelAlpha5 = accel.Select(fun (_,y,z) -> 5.0*200.0/Math.PI*Math.Atan2(float z, float y) |> int |> Some )

    use h = accelAlpha5.Subscribe(fun x -> printfn "%A" x)

    use h = accelAlpha5.Subscribe(arm) *)        
    
    log "Ready"
    model.AnalogSensor.["JA1"].Observable.Subscribe(printfn "%A") |> ignore
    //System.Threading.Thread.Sleep(60*1000)
    System.Console.ReadKey() |> ignore
   
    0
    
    