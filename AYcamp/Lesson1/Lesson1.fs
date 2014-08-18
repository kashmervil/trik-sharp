module Lesson1

open System
open Trik

[<NoComparison; NoEquality>]
type Robot() =

    let super = new Trik.Model()
    let motors = ["M1"; "M2"; "M3"; "M4"] |> List.map (fun x -> super.Motor.[x])
    let sensors = ["A1"; "A2"; "A3"] |> List.map (fun x -> super.AnalogSensor.[x])

    member self.Led = super.Led

    member self.MotorM1 with set p = motors.[0].SetPower p
    member self.MotorM2 with set p = motors.[1].SetPower p
   
    member self.SensorA1 = sensors.[0].Read()

    member self.Stop() = motors |> List.iter (fun x -> x.Stop())

    member self.Sleep(sec: float) = System.Threading.Thread.Sleep(int <| sec * 1000.)
    member self.Sleep(millisec: int) = System.Threading.Thread.Sleep(millisec)
    member self.Say(text) = Helpers.PostToShell <| "espeak -v russian_test -s 100 " + text
    
    interface IDisposable with
        member self.Dispose() = (super :> IDisposable).Dispose()
    

