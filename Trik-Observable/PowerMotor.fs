namespace Trik

open System
open Trik.Helpers

type PowerMotor(i2cCommandNumber) =
    let path = string i2cCommandNumber
    member x.SetPower p = Trik.Helpers.trikSpecific (fun() -> I2C.send i2cCommandNumber (limit -100 100 p) 1)
    interface IObserver<int> with
        member x.OnNext(data) = x.SetPower data
        member x.OnError e = x.SetPower 0
        member x.OnCompleted () = x.SetPower 0
    interface IDisposable with
        member x.Dispose() = 
            printfn "Disposing"
            x.SetPower 0
    
    