namespace Trik.Devices
open System
open Trik.Helpers
open Trik.Reactive

type Battery() = 
    member self.Read() = I2C.receive 0x26
    member self.ReadVoltage() = (float <| self.Read()) / 1023.0 * 3.3 * (7.15 + 2.37) / 2.37
    member self.ToObservable(refreshRate) = Observable.interval(TimeSpan.FromMilliseconds(float refreshRate)) 
                                            |> Observable.map (fun _ -> self.ReadVoltage())