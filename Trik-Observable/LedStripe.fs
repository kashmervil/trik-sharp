namespace Trik

open System
open Trik

type LedStripePorts = {
    red: int
    green: int
    blue: int
    ground: int
    }

type LedStripe(ports: LedStripePorts) =
    let i2cCommandNumbers = [|ports.red; ports.green; ports.blue|];
    do Helpers.I2C.send 0x9C ports.ground 1

    member x.SetPower ((r,g,b): int*int*int) = 
        Array.iter2 (fun x v -> printfn "%A" (r, g, b); Helpers.I2C.send x (Helpers.limit -100 100 v) 1) i2cCommandNumbers [|r; g; b|]
    member x.PowerOff() = 
        x.SetPower(0,0,0)
        Helpers.I2C.send ports.ground 0 1 
    interface IObserver<int*int*int> with
        member x.OnNext((r,g,b) : int*int*int) = x.SetPower(r,b,g)
        member x.OnError(e) = x.PowerOff()
        member x.OnCompleted() = x.PowerOff()
    interface IDisposable with 
        member x.Dispose() = 
            x.PowerOff()
            Helpers.I2C.send ports.ground 0 1




    
    