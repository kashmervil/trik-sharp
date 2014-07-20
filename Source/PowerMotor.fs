namespace Trik

open System
open Trik.Helpers


type PowerMotor(i2cCommandNumber) =
    let mutable power = 0
    ///Sends power in range -100 100 to specified PowerMotor.
    /// 0   - stands for STOP signal.
    ///-100 - max power for moving in one direction.
    /// 100 - max power for opposite another. 
    ///(You can reassemble your motor cable to make sure it's going right way without any code changing) 
    member self.Power
         with get() = power
         and set p = I2C.Send i2cCommandNumber (limit -100 100 p) 1; power <- p
    member self.Stop() = self.Power <- 0
    interface IObserver<int> with
        member self.OnNext(data) = self.Power <- data
        member self.OnError(e) = self.Stop()
        member self.OnCompleted() = self.Stop()
    interface IDisposable with
        member self.Dispose() = 
            self.Stop()
    
   