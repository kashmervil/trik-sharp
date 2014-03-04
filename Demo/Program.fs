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

let avg3 (buf:IList<int<prcnt>*int<prcnt>*int<prcnt>>) = 
        let x,y,z = buf.[0]
        let mutable x' = x
        let mutable y' = y
        let mutable z' = z
        for i = 1 to buf.Count do
            let x,y,z = buf.[i]
            x' <- x' + x
            y' <- y' + y
            z' <- z' + z
        done
        x'/buf.Count, y'/buf.Count, z'/buf.Count
        
let log s = printfn "%s" s

[<EntryPoint>]
let main _ =     
    log "Started"
    let model =  Model.Create "config.xml"
    log "Loaded"
    
    // Actuators/Observers
    let leds = new LedStripe(0x14, 0x15, 0x16, 0x17)
    let arm = model.Servo.["JE1"] 
    let hand =  model.Servo.["JE2"]
        
    // Sensors/Observables
    let accel = model.Accel.Observable |> lpf avg3
    let gyro = model.Gyro.Observable |> lpf avg3
    
    

    //let distance = model.AnalogSensor.["JA1"].Observable |> lpf (fun buf -> buf.Average()) 
    //use h = distance.Skip(10).Subscribe(fun x -> printfn "%d" x)


    let trafficLight ((x, y, z) as arg) =
        if x < z then LedColor.Red 
        elif  x < y  then LedColor.Orange 
        else LedColor.Green  
    
    use h = accel.Select(trafficLight).Subscribe(model.Led)
    
    let accelAlpha5 = accel.Select(fun (_,y,z) -> let a = int <| 5.0*200.0/Math.PI*Math.Atan2(float z, float y) in Some (a*1<prcnt>))

    use h = accelAlpha5.Subscribe(fun x -> printfn "%A" x)

    use h = accelAlpha5.Subscribe(arm)         
    
    log "Ready"
    
    //System.Threading.Thread.Sleep(60*1000)
    System.Console.ReadKey() |> ignore
   
    0
    
    