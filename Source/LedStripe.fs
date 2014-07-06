namespace Trik

open System
open Trik

/// Numbers of I2C ports associated with Stripe  
type LedStripePorts = {
    Red: int
    Green: int
    Blue: int
    Ground: int
    }

    
///<summary>Provides methods for working with Light Emitting Diode Stripe
///</summary>  

type LedStripe(ports: LedStripePorts) =
    let i2cCommandNumbers = [|ports.Red; ports.Green; ports.Blue|];
    do Helpers.I2C.Send ports.Ground 100 1 
    /// <summary>Sends specified (Red, Greed, Blue) color to a stripe.
    /// Each component is squished between -100 and 100</summary>
    member x.SetPower ((r,g,b): int*int*int) = 
        Array.iter2 (fun x v -> Helpers.I2C.Send x (Helpers.limit -100 100 v) 1) i2cCommandNumbers [|r; g; b|]
    /// <summary> Powers off a stripe </summary>
    member x.PowerOff() = 
        x.SetPower(0,0,0)
        Helpers.I2C.Send ports.Ground 0 1 
    interface IObserver<int*int*int> with
        member x.OnNext((r,g,b) : int*int*int) = x.SetPower(r,b,g)
        member x.OnError(e) = x.PowerOff()
        member x.OnCompleted() = x.PowerOff()
    interface IDisposable with 
        member x.Dispose() = 
            x.PowerOff()
            Helpers.I2C.Send ports.Ground 0 1





    
    