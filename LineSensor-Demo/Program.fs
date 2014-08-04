open Trik
printfn "Initial Start"

let model = new Model()
printfn "After constructing"

let r = model.LineSensor.ToObservable().Subscribe(fun x -> System.Console.WriteLine(x.ToString()))
model.LineSensor.Start()
printfn "Waiting for end"

open System.Threading

let _ = new Timer(new TimerCallback(fun _ -> model.LineSensor.DetectAndSet()
                                             printfn "\n\n\n\n\n\n\n"),null, 20000, 20000)

System.Console.ReadLine() |> ignore