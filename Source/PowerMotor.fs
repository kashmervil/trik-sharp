namespace Trik

open System
open Trik.Helpers


type PowerMotor(i2cCommandNumber) =
    let path = string i2cCommandNumber
    ///Sends power in range -100 100 to specified PowerMotor.
    /// 0   - stands for STOP signal.
    ///-100 - max power for moving in one direction.
    /// 100 - max power for opposite another. 
    ///(You can reassemble your motor cable to make sure it's going right way without any code changing) 
    member x.SetPower(power) = I2C.Send i2cCommandNumber (limit -100 100 power) 1
    interface IObserver<int> with
        member x.OnNext(data) = x.SetPower data
        member x.OnError(e) = x.SetPower 0
        member x.OnCompleted() = x.SetPower 0
    interface IDisposable with
        member x.Dispose() = 
            x.SetPower 0
    
   