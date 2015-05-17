module Lesson1

open System
open Trik

[<NoComparison; NoEquality>]
type Robot() =

    let super = new Trik.Model()
    let motors = super.Motors
    let sensors = super.AnalogSensors

    member self.Led = super.Led

    member self.MotorM1 with set p = motors.[M1].SetPower p
    member self.MotorM2 with set p = motors.[M2].SetPower p
   
    member self.SensorA1 = sensors.[A1].Read()

    member self.Stop() = motors.Values |> Seq.iter (fun x -> x.Stop())

    member self.Sleep(sec: float) = System.Threading.Thread.Sleep(int <| sec * 1000.)
    member self.Sleep(millisec: int) = System.Threading.Thread.Sleep(millisec)
    member self.Say(text) = Helpers.Shell.post <| "espeak -v russian_test -s 100 " + text
    
    interface IDisposable with
        member self.Dispose() = (super :> IDisposable).Dispose()
    

