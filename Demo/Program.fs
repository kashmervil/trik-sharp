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
open Microsoft.FSharp.Control

let lpf (avg:IList<'a>->'a) (o:IObservable<_>) = o.Buffer(5).Select(avg)

let avg3 (buf:IList<_>) = 
        let (x',y',z') = buf.Aggregate(fun (x,y,z) (x',y', z') -> (x + x', y + y', z+ z'))
        x'/buf.Count, y'/buf.Count, z'/buf.Count  
        
let log s = printfn "%s" s

[<EntryPoint>]
let main _ = 

    log "Started"
    Helpers.I2C.init "/dev/i2c-2" 0x48 1
    let model =  Model.Create "config.xml"
    log "Loaded"
    
    // Actuators/Observers
    let leds = new LedStripe(0x14, 0x15, 0x16, 0x17)
    let arm = model.Servo.["JE1"] 
    let hand =  model.Servo.["JE2"]
        
    // Sensors/Observables
    let accel = model.Accel.Observable |> lpf avg3
    let gyro = model.Gyro.Observable |> lpf avg3
    
    //.DistinctUntilChanged()
    let eps = 10; 
    //Observable.Generate(
    let observableGenerate (init: 'T) (iter: 'T -> 'T) (res: 'T -> 'R) (timeSelector: 'T -> DateTimeOffset)= 
        let subscriptions = ref (new HashSet< IObserver<'T> >())
        let thisLock = new Object()
        let stored = ref init
        let next(obs) = 
            (!subscriptions) |> Seq.iter (fun x ->  x.OnNext(obs) ) 
        let obs = 
            { new IObservable<'T> with
                member this.Subscribe(obs) =               
                    lock thisLock (fun () ->
                        (!subscriptions).Add(obs) |> ignore
                        )
                    { new IDisposable with 
                        member this.Dispose() = 
                            lock thisLock (fun () -> 
                                (!subscriptions).Remove(obs))  |> ignore } }
        Async.Start(async {
            next(!stored)
            stored := iter (!stored)
            Thread.Sleep(timeSelector)
            next(iter)
            return !loop
        })
        obs

    let distinctUntilChanged (sq: IObservable<'T>) : IObservable<'T> = 
        let prev = ref (None : 'T option)
        Observable.filter (fun x -> 
            match !prev with 
            | Some y when x = y -> false 
            | _ -> prev := Some(x); true            
            ) sq

    let clear =
        model.AnalogSensor.["JA1"].Observable
        |> Observable.map(fun x -> 
            //printfn "%A" x
            if x >= 20 then LedColor.Red else LedColor.Off
            ||| if x <= 50 then LedColor.Green else LedColor.Off
            )
        //|> Observable.scan (fun acc src -> if Math.Abs(acc - src) < eps then acc else src) Int32.MinValue
        |> distinctUntilChanged
        
    use h = clear.Subscribe model.Led 
    clear.Add <| printfn "%A"
    //|> Observable.add(fun x -> () )
    //let distance = model.AnalogSensor.["JA1"].Observable |> lpf (fun buf -> int <| buf.Average()) 
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
    //model.AnalogSensor.["JA1"].Observable.Subscribe(printfn "%A") |> ignore
    //System.Threading.Thread.Sleep(60*1000)
    System.Console.ReadKey() |> ignore
   
    0
    
    