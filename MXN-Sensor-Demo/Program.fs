open Trik
open System
open System.Threading

let exit = new EventWaitHandle(false, EventResetMode.AutoReset)

[<EntryPoint>]
let main _ = 
    let model = new Model()
    let buttons = new ButtonPad()
    use pleasework = new Trik.Sensors.MXNSensor(Ports.VideoSource.VP2)


    buttons.Start()
    pleasework.Start()

    use upButtonDispose = buttons.ToObservable()
                          |> Observable.filter (fun x -> ButtonEventCode.Up = x.Button)
                          |> Observable.subscribe (fun _ -> exit.Set() |> ignore)

    use leftButtonDispose = buttons.ToObservable()
                            |> Observable.filter (fun x -> ButtonEventCode.Left = x.Button)
                            |> Observable.subscribe (fun _ -> let (a, b) = pleasework.Size
                                                              pleasework.Size <- (a + 1, b)
                                                              printfn "resize first coordinate")
    use rightButtonDispose = buttons.ToObservable()
                            |> Observable.filter (fun x -> ButtonEventCode.Right = x.Button)
                            |> Observable.subscribe (fun _ -> let (a, b) = pleasework.Size
                                                              pleasework.Size <- (a, b + 1)
                                                              printfn "resize second coordinate")

    use lol = 
        pleasework.ToObservable()
        |> Observable.subscribe (fun x -> printfn "%A" x)

    pleasework.Size <- 1, 1

    exit.WaitOne() |> ignore
    0