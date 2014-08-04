namespace Trik.Junior

open System
open System.Collections.Generic
open Trik
open Trik.Ports

[<NoEquality; NoComparison>]
type Robot() as is =
    
    static let mutable isRobotAlive = false
    do if isRobotAlive then invalidOp "Only single instance is allowed"
    do isRobotAlive <- true

    let super = new Trik.Model()

    let mutable gyroValue: Point = Point.Zero
    let mutable accelValue: Point = Point.Zero

    static let resources = new ResizeArray<_>()
    
    let mutable isDisposed = false
    do AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> if not isDisposed then (is :> IDisposable).Dispose())
    
    member self.Led = new Trik.Led("/sys/class/leds/")

    member self.Motor = Ports.Motor.Values |> Array.map (fun port -> (port, new PowerMotor(port))) |> dict
    member self.Sensor = Ports.Sensor.Values |> Array.map (fun port -> (port, new AnalogSensor(port))) |> dict
    member self.Servo = 
        Ports.Servo.Values 
        |>  Array.map (fun port -> port, new ServoMotor(port.Path(), ServoMotor.Servo1)) |> dict
   
    member self.GyroRead() = super.Gyro.Read()
    
    member self.AccelRead() = super.Accel.Read()
    
    static member RegisterResource(d: IDisposable) = lock resources <| fun () -> resources.Add(d)

    member self.Stop() = self.Motor.Values |> Seq.iter (fun x -> x.Stop())
                         self.Servo.Values |> Seq.iter (fun x -> x.Zero()) 

    member self.Sleep(sec: float) = System.Threading.Thread.Sleep(int <| sec * 1000.)
    member self.Sleep(millisec: int) = System.Threading.Thread.Sleep(millisec)
    member self.Say(text) = Async.Start 
                            <| async { Trik.Helpers.SyscallShell <| "espeak -v russian_test -s 100 " + text}
    interface IDisposable with
        member self.Dispose() = 
            if not isDisposed then
                lock self 
                <| fun () -> 
                    isDisposed <- true
                    lock super 
                        <| fun () -> 
                        resources.ForEach(fun x -> x.Dispose())
                        (super :> IDisposable).Dispose()

    
[<AutoOpen>]
module Declarations =
    let robot = new Robot()