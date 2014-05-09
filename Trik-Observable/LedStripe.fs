namespace Trik

open System
open Trik

type LedStripe(rc,gc,bc, gnd) =
    let i2cCommandNumbers = [|rc; gc; bc|];
    do Helpers.I2C.send -100 gnd 1

    member x.SetPower ((r,g,b): int*int*int) = 
        Array.iter2 (fun x v -> Helpers.I2C.send x (Helpers.limit -100 100 v) 1) i2cCommandNumbers [|r; g; b|]
    member x.PowerOff() = 
        x.SetPower(0,0,0)
        Helpers.I2C.send gnd 0 1 
    interface IObserver<int*int*int> with
        member x.OnNext((r,g,b) : int*int*int) = x.SetPower(r,b,g)
        member x.OnError(e) = x.PowerOff()
        member x.OnCompleted() = x.PowerOff()
    interface IDisposable with 
        member x.Dispose() = 
            x.PowerOff()
            Helpers.I2C.send gnd 0 1




    
    