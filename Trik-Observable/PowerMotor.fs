namespace Trik.Observable
open System
open Trik
type PowerMotor(i2cCommandNumber) =
    member x.SetPower (p:int<prcnt>) = Helpers.trikSpecific (fun() -> Helpers.I2C.send i2cCommandNumber (int p) 1)
    member x.PowerOff() = x.SetPower 0<prcnt>
    interface IObserver<int<prcnt>> with
        member x.OnNext(data) = x.SetPower data
        member x.OnError e = x.PowerOff()
        member x.OnCompleted () = x.PowerOff()
    interface IDisposable with
        member x.Dispose() = x.PowerOff()
    
    