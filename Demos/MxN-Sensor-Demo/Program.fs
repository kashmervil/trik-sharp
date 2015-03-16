open System
open System.Threading
open Trik
open Trik.Devices
open Trik.Collections

let exit = new EventWaitHandle(false, EventResetMode.AutoReset)

[<EntryPoint>]
let main _ = 
    let model = new Model()
    let buttons = model.Buttons
    use sensor = model.MXNSensor
    
    buttons.Start()
    sensor.Start()

    use upButtonDispose = buttons.ToObservable()
                          |> Observable.filter (fun x -> ButtonEventCode.Up = x.Button)
                          |> Observable.subscribe (fun _ -> exit.Set() |> ignore)

    use leftButtonDispose = buttons.ToObservable()
                            |> Observable.filter (fun x -> ButtonEventCode.Left = x.Button)
                            |> Observable.subscribe (fun _ -> let (a, b) = sensor.Size
                                                              sensor.Size <- (a + 1, b)
                                                              printfn "resize first coordinate")
    use rightButtonDispose = buttons.ToObservable()
                            |> Observable.filter (fun x -> ButtonEventCode.Right = x.Button)
                            |> Observable.subscribe (fun _ -> let (a, b) = sensor.Size
                                                              sensor.Size <- (a, b + 1)
                                                              printfn "resize second coordinate")

    use lol = 
        sensor.ToObservable()
        |> Observable.subscribe (fun x -> printfn "%A" x)

    sensor.Size <- 1, 1

    exit.WaitOne() |> ignore
    0