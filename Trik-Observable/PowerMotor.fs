namespace Trik.Observable

open System
type PowerMotor(i2cCommandNumber) =
    interface IObserver<int> with
        member this.OnNext(data) = Trik.Helpers.trikSpecific (fun() -> Trik.Helpers.I2C.send i2cCommandNumber data 1)
        member this.OnError e = ()
        member this.OnCompleted () = ()
    
    