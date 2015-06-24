namespace Trik.Sensors
open System
open Trik


[<Sealed>]
type LineSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit Internals.VideoSensor<LineLocation>(scriptPath, commandPath, sensorPath)
    let mutable isDisposed = false
    new (videoSource) = 
        let script = 
            match videoSource with
            | VideoSource.USB -> "/etc/init.d/line-sensor-webcam.sh"
            | _                     -> "/etc/init.d/line-sensor-ov7670.sh"

        new LineSensor(script, "/run/line-sensor.in.fifo", "/run/line-sensor.out.fifo")

    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> LineLocation(x, y, z) |> Location |> Some
                | [| "hsv:"; h; s; v; ht; st; vt |] -> DetectTarget(h,s,v,ht,st,vt) |> Target |> Some
                | z -> System.Console.WriteLine("object sensor parse error! None {0}", z); None
   
    override self.Dispose() = base.Dispose()

    override self.Finalize() = self.Dispose()