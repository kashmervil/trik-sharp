open Trik

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

let ledstripeDisposable = colorStream.Subscribe model.LedStripe

let setterDisposable = targetStream |> Observable.subscribe sensor.SetDetectTarget


let locationStream = sensorOutput  
                     |> Observable.choose (fun o -> o.TryGetLocation) 
                     |> Observable.map (fun x -> x.X)

let motorDispose = locationStream.Subscribe model.Motor.["M1"] 

let downButtonDispose = buttons.ToObservable() 
                        |> Observable.filter (fun x -> ButtonEventCode.Down = x.Button) 
                        |> Observable.subscribe (fun _ -> printfn "Detect by pressing Down";sensor.Detect())

let timerSetterDisposable = Observable.Interval(System.TimeSpan.FromSeconds 7.0) 
                            |> Observable.subscribe (fun _ -> printfn "Usual Detect by timer"; sensor.Detect())

printfn "press any key to detect"

while buttons.Read().Button <> ButtonEventCode.Up do
    printfn """Press "Up" to stop """