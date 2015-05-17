open Trik
open Trik.Collections
open System.Threading

let maxAngleX = 60
let maxAngleY = 60
let scaleConstX = 20
let scaleConstY = 20
let RGBdepth = 100.
let minMass = 5

let red = (100, 0, 0)
let green = (0, 100, 0)
let blue = (0, 0, 100)
let brown = (65, 17, 17)
let orange = (100, 65, 0)
let yellow = (100, 100, 0)
let teal = (0, 50, 50)
let purple = (63, 13, 94)
let pink = (100, 75, 80)
let colors = 
    [red; green; blue; brown; orange; yellow; teal; purple; pink]

let scale var mA sC  = (Trik.Helpers.limit (-mA) mA var) / sC

let updatePositionX x acc = 
    if (x > 15 && x <= 100 && acc < 90) || (x < -15 && x >= -100 && acc > -90) 
    then acc + scale x maxAngleX scaleConstX
    else acc

let updatePositionY y acc =
    if (y > 5 && y <= 100 && acc < 30) || (y < -5 && y >= -100 && acc > -30) 
    then acc + scale y maxAngleY scaleConstY
    else acc

let colorProcessor (r, g, b) = 
    let del (x, y, z) = (x - r) * (x - r) + (g - y) * (g - y) + (b - z) * (b - z)
    let rec loop (x :: xs) acc =
        let sup = del x
        let accDelta = del acc
        match (x :: xs) with
        | [t] -> printfn "t : %A ;; acc : %A" t acc; if del t < accDelta then t else acc 
        | x :: xs when sup < accDelta -> loop xs x
        | x :: xs when sup >= accDelta -> loop xs acc   
        | _ -> failwith "no way" 
    loop colors red

let conversion (x : DetectTarget) = 
    let (r, g, b) = 
        Trik.Helpers.HSVtoRGB(float x.Hue, (float x.Saturation) / 100. , (float x.Value) / 100.)
    colorProcessor (int (r * RGBdepth), int (g * RGBdepth), int (b * RGBdepth))

let exit = new EventWaitHandle(false, EventResetMode.AutoReset)

let servoSetting = { stop = 0; zero = 1400000; min = 625000; max = 2175000; period = 20000000 }

[<EntryPoint>]
let main _ =
    
    use model = new Model(ObjectSensorConfig = VideoSource.USB)
    model.ServosConfig.[C1] <- servoSetting
    model.ServosConfig.[C2] <- servoSetting

    use sensor  = model.ObjectSensor
    use buttons = model.Buttons
    use xServo  = model.Servos.[C1]
    use yServo  = model.Servos.[C2]

    let sensorOutput = sensor.ToObservable()
    
    let targetStream = sensorOutput |> Observable.choose (fun o -> o.TryGetTarget) 

    use setterDisposable = targetStream |> Observable.subscribe sensor.SetDetectTarget

    let colorStream = targetStream |> Observable.map conversion

    use ledstripeDisposable = colorStream.Subscribe model.LedStripe

    use powerSetterDisposable = 
             model.ObjectSensor.ToObservable()  
             |> Observable.choose (fun o -> o.TryGetLocation)
             |> Observable.filter (fun loc -> loc.Mass > minMass) 
             |> Observable.scan (fun (accX, accY) loc -> (updatePositionX loc.X accX, updatePositionY loc.Y accY)) (0, 0)
             |> Observable.subscribe (fun (a, b) -> xServo.SetPower -a
                                                    yServo.SetPower b)
    
    let observableButtons = buttons.ToObservable()
    use downButtonDispose = observableButtons
                            |> Observable.filter (fun x -> ButtonEventCode.Down = x.Button) 
                            |> Observable.subscribe (fun _ -> sensor.Detect())
    
    use upButtonDispose = observableButtons
                          |> Observable.filter (fun x -> ButtonEventCode.Up = x.Button)
                          |> Observable.subscribe (fun _ -> exit.Set() |> ignore)

//    use timerSetterDisposable = Observable (System.TimeSpan.FromSeconds 40.0) 
//                                |> Observable.subscribe (fun _ -> sensor.Detect())
    
    buttons.Start()
    sensor.Start()
    Trik.Helpers.SendToShell """v4l2-ctl -d "/dev/video2" --set-ctrl white_balance_temperature_auto=1"""

    model.LedStripe.SetPower (75, 20, 20)

    exit.WaitOne() |> ignore
    0
