namespace Trik.Junior

open System
open Trik
open Trik.Network
open Trik.Helpers

[<NoEquality; NoComparison>]
type Robot() as is =
    static let mutable isRobotAlive = false
    do if isRobotAlive then invalidOp "Only single instance is allowed"
    do isRobotAlive <- true

    let mutable isDisposed = false
    do AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> if not isDisposed then (is :> IDisposable).Dispose())
    
    let super = new Trik.Model()
    static let resources = new ResizeArray<_>()
    
    let takePicture name = 
        Shell.send <| "v4l2grab -d \"/dev/video2\" -H 640 -W 480 -o " + name + " 2> /dev/null"
    let defaultName() = 
        let date = DateTime.Now
        "trik-cam-" + date.ToString("yyMMdd-HHmmss.jp\g")

    member val Buttons = super.Buttons
    member val Uploader = new FtpClient()

    member val Led = super.Led

    member val Encoder = super.Encoders
    member val Motor = super.Motors
    member val Sensor = super.AnalogSensors
    member val Servo = super.Servos
   
    member val Gyro = super.Gyro
    
    member val Accel = super.Accel
    
    member val LineSensor = super.LineSensor
    
    member self.Stop() = self.Motor.Values |> Seq.iter (fun x -> x.Stop())
                         self.Servo.Values |> Seq.iter (fun x -> x.Zero()) 
    
    member self.Sleep(sec: float) = System.Threading.Thread.Sleep(int <| sec * 1000.)
    member self.Sleep(millisec: int) = System.Threading.Thread.Sleep(millisec)
    
    member self.Say(text) = 
        Shell.post <| "espeak -v russian_test -s 100 \"" + text + "\" 2> /dev/null"
    
    member self.PlayFile(file:string) = 
        Shell.post <| 
            if file.EndsWith(".wav") then "aplay --quiet &quot;" + file + "&quot; &amp;"
            elif file.EndsWith(".mp3") then "cvlc --quiet &quot;" + file + "&quot; &amp;"
            else invalidArg "file" "Incorrect filename"

    member self.TakeScreenshot() = 
        Shell.post <|
        let date = DateTime.Now
        "fbgrab trik-screenshot-" + date.ToString("yyMMdd-HHmmss.pn\g") + " 2> /dev/null"

    member self.TakePicture() = let name = defaultName() in name |> takePicture; name
    
    member self.TakePicture name = name |> takePicture |> ignore

    
    member self.ShotAndUpload() = 
            let name = defaultName()
            takePicture name
            self.Uploader.AsyncUpload name |> Async.Start
             
    static member RegisterResource(d: IDisposable) = lock resources <| fun () -> resources.Add(d)

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