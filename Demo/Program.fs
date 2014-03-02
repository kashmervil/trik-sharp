module Program

open System
open System.Diagnostics
open System.Threading
open System.Collections.Generic
open System.Reactive.Linq
open System.Reactive.Joins
open System.Linq
open Trik
open Trik.Observable

let lpf (avg:IList<'a>->'a) (o:IObservable<_>) = o.Buffer(5).Select(avg)

let avg3 (buf:IList<_>) = 
        let (x',y',z') = buf.Aggregate(fun (x,y,z) (x',y', z') -> (x + x', y + y', z+ z'))
        x'/buf.Count, y'/buf.Count, z'/buf.Count  
        


[<EntryPoint>]
let main _ = 
    let model =  Model.Create "config.xml"
    let leds = new LedStripe(0x14, 0x15, 0x16, 0x17)
    let hand =  model.Servo.["JE1"]
    let arm = model.Servo.["JE2"] 
        

    let accel = model.Accel.Observable |> lpf avg3
    let gyro = model.Gyro.Observable |> lpf avg3
    let distance = model.AnalogSensor.["JA1"].Observable |> lpf (fun buf -> int <| buf.Average())
    
    let trafficLight d = if d < 300 then LedColor.Red elif  d < 500  then LedColor.Orange else LedColor.Green  
    use h = distance.Select(trafficLight).DistinctUntilChanged().Subscribe(model.Led)
    
    
    let accelAlpha5 = accel.Select(fun (x,_,z) -> int <| 5.0*200.0/Math.PI*Math.Atan2(float z, float x))
    use h = accel.Subscribe(leds) 
        
        
    System.Console.ReadKey() |> ignore
    0
    
    