namespace Trik.Devices
open System
open Trik
open Trik.Helpers
open Trik.Collections

///<summary>Provides methods for working with Light Emitting Diode Stripe
///</summary>  

type LedStripe(ports: LedStripePorts) =
    let i2cCommandNumbers = [|ports.Red; ports.Green; ports.Blue|];
    do I2C.send ports.Ground 100 1 
    /// <summary>Sends specified (Red, Greed, Blue) color to a stripe.
    /// Each component is squished between -100 and 100</summary>
    member x.SetPower ((r,g,b): int*int*int) = 
        Array.iter2 (fun x v -> I2C.send x (Calculations.limit -100 100 v) 1) i2cCommandNumbers [|r; g; b|]
    /// <summary> Powers off a stripe </summary>
    member x.PowerOff() = 
        x.SetPower(0,0,0)
        I2C.send ports.Ground 0 1
    
    override self.Finalize() = (self :> IDisposable).Dispose()
         
    interface IObserver<int*int*int> with
        member x.OnNext((r,g,b) : int*int*int) = x.SetPower(r,b,g)
        member x.OnError(e) = x.PowerOff()
        member x.OnCompleted() = x.PowerOff()
    
    interface IDisposable with 
        member x.Dispose() = 
            x.PowerOff()
            I2C.send ports.Ground 0 1





    
    