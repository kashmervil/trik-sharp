open System
open Trik

let model = new Model()
let inline f (coord: Point) = 
    (100 - abs(coord.X/10) - abs(coord.Z/10), abs(coord.X/10) + abs(coord.Y/10), abs(coord.Z/10) + abs(coord.Y/10))  
let disp = model.Gyro.ToObservable().Subscribe(f >> model.LedStripe.SetPower)

model.Gyro.Start()
printfn "Press any key to stop"
Console.ReadKey() |> ignore


