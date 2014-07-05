open System
open Trik
[<EntryPoint>]
let main argv = 
    Helpers.I2C.Init "/dev/i2c-2" 0x48 1
    use model = new Model()
//model.LedStripe |> ignore
    let inline f (x:int array) = 
        (100 - abs(x.[0]/10) - abs(x.[2]/10), abs(x.[0]/10) + abs(x.[1]/10), abs(x.[2]/10) + abs(x.[1]/10))  
    use disp = model.Gyro.ToObservable().Subscribe(
                     fun (x: int array) -> model.LedStripe.SetPower(f x))

    Console.ReadKey() |> ignore
    0 // return an integer exit code
