namespace Trik.Devices
open System
open Trik
open Trik.Helpers

type PowerMotor(i2cCommandNumber) =
    ///Sends power in range -100 100 to specified PowerMotor.
    /// 0   - stands for STOP signal.
    ///-100 - max power for moving in one direction.
    /// 100 - max power for opposite another. 
    ///(You can reassemble your motor cable to make sure it's going right way without any code changing
    member self.SetPower(power) = I2C.send i2cCommandNumber (Calculations.limit -100 100 power) 1
    member self.Stop() = self.SetPower 0
    
    new (port: IMotorPort) = new PowerMotor(port.I2CNumber)

    override self.Finalize() = (self :> IDisposable).Dispose()

    interface IObserver<int> with
        member self.OnNext(data) = self.SetPower data
        member self.OnError(e) = self.Stop()
        member self.OnCompleted() = self.Stop()
    
    interface IDisposable with
        member self.Dispose() = 
            self.Stop()