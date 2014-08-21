namespace Trik.Junior

open System
open Trik
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
        SendToShell <| "v4l2grab -d \"/dev/video2\" -H 640 -W 480 -o " + name + " 2> /dev/null"
    let pictureName() = 
        let date = DateTime.Now
        "trik-cam-" + date.ToString("yyMMdd-HHmmss.jp\g")

    let stopwatch = new System.Diagnostics.Stopwatch()
    member val ButtonPad = new ButtonPad("/dev/input/event0")
    member val Uploader = new FtpClient()

    member self.Led = super.Led

    member self.Motor = super.Motor.Values |> Seq.mapi (fun i x -> Ports.Motor.Values.[i], x) |> dict
    member self.Sensor = super.AnalogSensor.Values |> Seq.mapi (fun i x -> Ports.Sensor.Values.[i], x) |> dict
    member self.Servo = super.Servo.Values |> Seq.mapi (fun i x -> Ports.Servo.Values.[i], x) |> dict
   
    member self.Gyro = super.Gyro
    
    member self.Accel = super.Accel
    
    member self.Stop() = self.Motor.Values |> Seq.iter (fun x -> x.Stop())
                         self.Servo.Values |> Seq.iter (fun x -> x.Zero()) 
    
    member self.Encoder = super.Encoder
    member self.LineSensor = super.LineSensor
    member self.Sleep(sec: float) = System.Threading.Thread.Sleep(int <| sec * 1000.)
    member self.Sleep(millisec: int) = System.Threading.Thread.Sleep(millisec)
    
    member self.Say(text) = 
        PostToShell <| "espeak -v russian_test -s 100 \"" + text + "\" 2> /dev/null"
    
    member self.PlayFile (file:string) = 
        PostToShell <| 
            if file.EndsWith(".wav") then "aplay --quiet &quot;" + file + "&quot; &amp;"
            elif file.EndsWith(".mp3") then "cvlc --quiet &quot;" + file + "&quot; &amp;"
            else invalidArg "file" "Incorrect filename"

    member self.TakeScreenshot() = 
        
        PostToShell <|
        let date = DateTime.Now
        "fbgrab trik-screenshot-" + date.ToString("yyMMdd-HHmmss.pn\g") + " 2> /dev/null"

    member self.TakePicture() = pictureName() |> takePicture// |> Async.Start
    
    member self.ShotAndUpload() = 
            
            let name = pictureName()
            stopwatch.Restart()
            do takePicture name
            printfn "shooting took %d" stopwatch.ElapsedMilliseconds
            stopwatch.Restart()
            do self.Uploader.Upload name
            printfn "uploading took %d" stopwatch.ElapsedMilliseconds
             
        //}  |> Async.RunSynchronously
        
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