open Trik
open Trik.Helpers
open Trik.Collections
open Trik.Reactive
open System.Threading

let maxAcc      = 90
let maxAngleX   = 60
let maxAngleY   = 60
let scaleConstX = 20
let scaleConstY = 20
let RGBdepth    = 100.
let minMass     = 5

let red    = (100, 0, 0)
let green  = (0, 100, 0)
let blue   = (0, 0, 100)
let brown  = (65, 17, 17)
let orange = (100, 65, 0)
let yellow = (100, 100, 0)
let teal   = (0, 50, 50)
let purple = (63, 13, 94)
let pink   = (100, 75, 80)
let colors = 
    [red; green; blue; brown; orange; yellow; teal; purple; pink]

let scale var border  = limit (-border) border var

let updatePositionX x = (scale x maxAngleX) / scaleConstX

let updatePositionY y = (scale y maxAngleY) / scaleConstY

let colorProcessor (r, g, b) = 
    let del (x, y, z) = (x - r) * (x - r) + (g - y) * (g - y) + (b - z) * (b - z)
    colors |> List.fold (fun acc x -> let xDelta = del x
                                      let accDelta = del acc 
                                      if xDelta < accDelta then x else acc) red

let conversion (x : DetectTarget) = 
    let (r, g, b) = 
        ColorSpaces.HSVtoRGB(float x.Hue, (float x.Saturation) / 100. , (float x.Value) / 100.)
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

    let sensorOutput = sensor.ToObservable()
    
    let targetStream = sensorOutput |> Observable.choose (fun o -> o.TryGetTarget) 

    use setterDisposable = targetStream |> Observable.subscribe sensor.SetDetectTarget

    let colorStream = targetStream |> Observable.map conversion

    use ledstripeDisposable = colorStream.Subscribe model.LedStripe

    let locationStream = sensor.ToObservable()  
                         |> Observable.choose (fun o -> o.TryGetLocation)
                         |> Observable.filter (fun loc -> loc.Mass > minMass)

    use xPowerSetter = locationStream 
                       |> Observable.map (fun loc -> -loc.X)
                       |> Observable.scan (fun acc loc -> scale maxAcc acc + updatePositionX loc) 0
                       |> Observable.subscribe model.Servos.[C1].SetPower

    use yPowerSetter = locationStream
                       |> Observable.map (fun loc -> loc.Y)
                       |> Observable.scan (fun acc loc -> scale maxAcc acc + updatePositionY loc) 0
                       |> Observable.subscribe model.Servos.[C2].SetPower
    
    let observableButtons = buttons.ToObservable()
    use downButtonDispose = observableButtons
                            |> Observable.filter (fun x -> ButtonEventCode.Down = x.Button) 
                            |> Observable.subscribe (fun _ -> sensor.Detect())
    
    use upButtonDispose = observableButtons
                          |> Observable.filter (fun x -> ButtonEventCode.Up = x.Button)
                          |> Observable.subscribe (fun _ -> exit.Set() |> ignore)

    use timerSetterDisposable = Observable.interval (System.TimeSpan.FromSeconds 40.0) 
                                |> Observable.subscribe (fun _ -> sensor.Detect())
    
    buttons.Start()
    sensor.Start()
    Helpers.Shell.send  """v4l2-ctl -d "/dev/video2" --set-ctrl white_balance_temperature_auto=1"""

    model.LedStripe.SetPower (75, 20, 20)

    exit.WaitOne() |> ignore
    0
