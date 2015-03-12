namespace Trik.Devices
open Trik
    type Battery() = 
        member self.Read() = Trik.Helpers.I2C.Receive 0x26
        member self.ReadVoltage() = (float <| self.Read()) / 1023.0 * 3.3 * (7.15 + 2.37) / 2.37
        member self.ToObservable(refreshRate) = Observable.Interval(System.TimeSpan.FromMilliseconds(float refreshRate)) |> Observable.map (fun _ -> self.ReadVoltage())