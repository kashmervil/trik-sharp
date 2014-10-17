open Trik

let model = new Model(ObjectSensorConfig = Ports.VideoSource.USB)

let sensor = model.ObjectSensor

let buttons = new ButtonPad()

let sensorOutput = sensor.ToObservable()

let targetStreame = sensorOutput  
                    |> Observable.choose (fun o -> o.TryGetTarget) 


let colorStream = targetStreame |> Observable.map (fun x -> (x.Hue, x.Value, x.Saturation))

let ledstripeDisposable = colorStream.Subscribe model.LedStripe

let setterDisposable = targetStreame |> Observable.subscribe sensor.SetDetectTarget


let locationStream = sensorOutput  
                     |> Observable.choose (fun o -> o.TryGetLocation) 
                     |> Observable.map (fun x -> x.X)

let motorDispose = locationStream.Subscribe model.Motor.["M1"] 

let downButtonDispose = buttons.ToObservable() |> Observable.filter (fun x -> ButtonEventCode.Down = x.Button) |> Observable.subscribe (fun _ -> sensor.Detect())

let timerSetterDisposable = Observable.Interval(System.TimeSpan.FromSeconds 3.0) |> Observable.subscribe (fun _ -> sensor.Detect())


sensor.Start()
buttons.Start()

printfn "press any key to detect"

while buttons.Read().Button <> ButtonEventCode.Up do
    printfn """Press "Up" to stop """
