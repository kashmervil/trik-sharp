open Trik
open Trik.Helpers
open Trik.Collections
open System.Threading

let maxAngle = 40
let scaleConst = 10
let RGBdepth = 255.
let SVscale = 100.
let minMass = 5
let deepPink = (255, 20, 147)

let updatePosition x acc = 
    let scale x = (Trik.Helpers.limit (-maxAngle) maxAngle x) / scaleConst
    if (x > 10 && x < 90 && acc < 90) || (x < - 10 && x > -90 && acc > -90) 
    then scale x + acc 
    else acc

let conversion (x : DetectTarget) = 
    let (r, g, b) = HSVtoRGB(float x.Hue, (float x.Saturation) / SVscale, (float x.Value) / SVscale)
    (int (r * RGBdepth), int (g * RGBdepth), int (b * RGBdepth))

let exit = new EventWaitHandle(false, EventResetMode.AutoReset)

[<EntryPoint>]
let main _ =
    let model = new Model(ObjectSensorConfig = VideoSource.USB)
    model.ServoConfig.[0] <- (E1, ("/sys/class/pwm/ehrpwm.1:1", Defaults.Servo3))
    Helpers.SendToShell """v4l2-ctl -d "/dev/video2" --set-ctrl white_balance_temperature_auto=1""" //option for better color reproduction

    let sensor = model.ObjectSensor
    let buttonPad = model.ButtonPad

    buttonPad.Start()
    sensor.Start()
    model.LedStripe.SetPower deepPink

    let sensorOutput = sensor.ToObservable()
    
    let targetStream = sensorOutput |> Observable.choose (fun o -> o.TryGetTarget) 

    use setterDisposable = targetStream |> Observable.subscribe sensor.SetDetectTarget

    let colorStream = targetStream |> Observable.map (fun x -> conversion x)

    use ledstripeDisposable = colorStream.Subscribe model.LedStripe

    let powerSetterDisposable = 
             model.ObjectSensor.ToObservable()  
             |> Observable.choose (fun o -> o.TryGetLocation)
             |> Observable.filter (fun l -> l.Mass > 5) 
             |> Observable.scan (fun acc l -> updatePosition l.X acc) 0
             |> Observable.subscribe (fun x -> model.Servo.[E1].SetPower -x)

    use downButtonDispose = buttonPad.ToObservable() 
                            |> Observable.filter (fun x -> ButtonEventCode.Down = x.Button) 
                            |> Observable.subscribe (fun _ -> sensor.Detect())
    
    use upButtonDispose = buttonPad.ToObservable()
                          |> Observable.filter (fun x -> ButtonEventCode.Up = x.Button)
                          |> Observable.subscribe (fun _ -> printfn "Exiting..."; exit.Set() |> ignore)

//    use timerSetterDisposable = Observable.Interval(System.TimeSpan.FromSeconds 7.0) 
//                                |> Observable.subscribe (fun _ -> (*printfn "Usual Detect by timer";*) sensor.Detect())
    exit.WaitOne() |> ignore
    0
