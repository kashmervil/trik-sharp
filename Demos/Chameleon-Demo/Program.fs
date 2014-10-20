open Trik
open System.Threading

[<EntryPoint>]
let main _ =
    let exit = new EventWaitHandle(false, EventResetMode.AutoReset)
    let model = new Model(ObjectSensorConfig = Ports.VideoSource.USB)
    let sensor = model.ObjectSensor
    sensor.Start()

    let buttons = new ButtonPad()
    buttons.Start()

    let sensorOutput = sensor.ToObservable()

    let targetStream = sensorOutput  
                        |> Observable.choose (fun o -> o.TryGetTarget) 

    let colorStream = targetStream 
                      |> Observable.map (fun x -> printfn "print from colorStream %s" <| x.ToString(); (x.Hue, x.Value, x.Saturation))

    use ledstripeDisposable = colorStream.Subscribe model.LedStripe

    use setterDisposable = targetStream |> Observable.subscribe sensor.SetDetectTarget

    let locationStream = sensorOutput  
                         |> Observable.choose (fun o -> o.TryGetLocation) 
                         |> Observable.map (fun x -> x.X)

    use motorDispose = locationStream.Subscribe model.Motor.["M1"] 


    use downButtonDispose = buttons.ToObservable() 
                            |> Observable.filter (fun x -> ButtonEventCode.Down = x.Button) 
                            |> Observable.subscribe (fun _ -> printfn "Detect by pressing Down";sensor.Detect())
    
    use upButtonDispose = buttons.ToObservable()
                            |> Observable.filter (fun x -> ButtonEventCode.Up = x.Button)
                            |> Observable.subscribe (fun _ -> printfn "Exiting..."; exit.Set() |> ignore)

    use timerSetterDisposable = Observable.Interval(System.TimeSpan.FromSeconds 7.0) 
                                |> Observable.subscribe (fun _ -> printfn "Usual Detect by timer"; sensor.Detect())
    
    exit.WaitOne() |> ignore
    0