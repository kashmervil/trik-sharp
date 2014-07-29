module Trik.Junior

open System
open System.Collections.Generic
open Trik.Tasking
type Robot() =
    
    let super = new Trik.Model()
    let motors = ["M1"; "M2"; "M3"; "M4"] |> List.map (fun x -> super.Motor.[x])
    let servos = ["E1"; "E2"] |> List.map (fun x -> super.Servo.[x])
    let sensors = ["A1"; "A2"; "A3"] |> List.map (fun x -> super.AnalogSensor.[x])

    let tasks = new ResizeArray<IDisposable>()
    
    member self.Led 
        with get() =  super.Led.Color
        and set c = super.Led.Color <- c

    member self.MotorM1 with set p = motors.[0].Power <- p
    member self.MotorM2 with set p = motors.[1].Power <- p
    member self.MotorM3 with set p = motors.[2].Power <- p
    member self.MotorM4 with set p = motors.[3].Power <- p
    
    member self.SensorA1 = sensors.[0].Read()
    member self.SensorA2 = sensors.[1].Read()
    member self.SensorA3 = sensors.[2].Read()

    member self.ServoE1 with set p = servos.[0].Power <- p
    member self.ServoE2 with set p = servos.[1].Power <- p

    member self.TaskStart(task: ReadyToStart<_>) = lock self <| fun () -> tasks.Add(Task.Start(task))

    member self.Stop() = motors |> List.iter (fun x -> x.Stop())
                         servos |> List.iter (fun x -> x.Zero())

    member self.Sleep(sec: float) = System.Threading.Thread.Sleep(int <| sec * 1000.)
    member self.Sleep(millisec: int) = System.Threading.Thread.Sleep(millisec)
    member self.Say(text) = Async.Start 
                            <| async { Helpers.SyscallShell <| "espeak -v russian_test -s 100 " + text}
    interface IDisposable with
        member self.Dispose() = (super :> IDisposable).Dispose(); tasks.ForEach(fun x -> x.Dispose())
    


