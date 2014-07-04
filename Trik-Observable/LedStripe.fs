namespace Trik

open System
open Trik


///<summary>Provides types for working with Light Emitting Diodes 
///e.g LedStripe or single led on top of a controller board
///</summary>  

type LedStripePorts = {
    Red: int
    Green: int
    Blue: int
    Ground: int
    }

type LedStripe(ports: LedStripePorts) =
    let i2cCommandNumbers = [|ports.Red; ports.Green; ports.Blue|];
    do Helpers.I2C.send ports.Ground 100 1 

    member x.SetPower ((r,g,b): int*int*int) = 
        Array.iter2 (fun x v -> Helpers.I2C.send x (Helpers.limit -100 100 v) 1) i2cCommandNumbers [|r; g; b|]
    member x.PowerOff() = 
        x.SetPower(0,0,0)
        Helpers.I2C.send ports.Ground 0 1 
    interface IObserver<int*int*int> with
        member x.OnNext((r,g,b) : int*int*int) = x.SetPower(r,b,g)
        member x.OnError(e) = x.PowerOff()
        member x.OnCompleted() = x.PowerOff()
    interface IDisposable with 
        member x.Dispose() = 
            x.PowerOff()
            Helpers.I2C.send ports.Ground 0 1





    
    