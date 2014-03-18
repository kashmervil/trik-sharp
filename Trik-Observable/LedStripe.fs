namespace Trik

open System
open Trik

type LedStripe(rc,gc,bc, gnd) =
    let i2cCommandNumbers = [|rc; gc; bc|];
    do Helpers.I2C.send -100 gnd 1

    member x.SetPower ((r,g,b): int*int*int) = 
        Array.iter2 (fun x v -> Helpers.I2C.send x (Helpers.limit -100 100 v) 1) i2cCommandNumbers [|r; g; b|] 
    interface IObserver<int*int*int> with
        member this.OnNext((r,g,b) : int*int*int) = this.SetPower(r,b,g)
        member this.OnError(e) = ()
        member this.OnCompleted() = ()
    interface IDisposable with 
        member x.Dispose() = ()


    
    