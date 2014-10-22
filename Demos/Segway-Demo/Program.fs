open Trik
//Thanks for Dima Soldatov for implementation
let model = new Model()

let sensor = model.AnalogSensor.["A3"]
let rWheel = model.Motor.["M4"]
let lWheel = model.Motor.["M3"]

let zeroD = 701.
let minPow = 2.

let sign a = if a > 0. then 1. elif a < 0. then -1. else 0.

let mutable d = 0.
let mutable dold = 0.
let mutable yaw = 0.

let pk = 0.7
let dk = 1.
let ik = 0.002

let loop = async {
    while true do
        d <- zeroD - (0.1*dold + 0.9* float(sensor.Read()))
        yaw <- sign(d)*minPow + d*pk + (d-dold)*dk + (d+dold)*ik
        dold <- d
        //printfn "%f %f" d yaw
        lWheel.SetPower (int yaw)
        rWheel.SetPower (int yaw)
        do! Async.Sleep 5
      }

let cts = new System.Threading.CancellationTokenSource()

Async.Start(loop, cts.Token)

System.Console.ReadLine() |> ignore

System.Console.WriteLine("Stopping model. Press any key to exit")
cts.Cancel()
System.Console.ReadKey() |> ignore