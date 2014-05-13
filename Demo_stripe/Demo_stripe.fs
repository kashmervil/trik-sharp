open System
open Trik
[<EntryPoint>]
let main argv = 

    let model = new Model()
    model.LedStripe |> ignore
    use disp = model.Gyro.ToObservable().Subscribe(
                     fun (x: int array) -> model.LedStripe.SetPower(x.[0]/100, x.[1]/100, x.[2]/100))
    Console.ReadKey() |> ignore
    0 // return an integer exit code
