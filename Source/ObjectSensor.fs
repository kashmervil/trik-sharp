namespace Trik.Sensors
open System
open Trik
open Trik.Collections

[<Sealed>]
type ObjectSensor(scriptPath, commandPath: string, sensorPath) = 
    inherit Internals.VideoSensor<ObjectLocation>(scriptPath, commandPath, sensorPath)
    let mutable isDisposed = false

    new (videoSource) = 
        let script = 
            match videoSource with
            | Ports.VideoSource.USB -> "/etc/init.d/object-sensor-webcam.sh"
            | _                     -> "/etc/init.d/object-sensor-ov7670.sh"

        new ObjectSensor(script, "/run/object-sensor.in.fifo", "/run/object-sensor.out.fifo")

    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> ObjectLocation(x, y, z) |> Location |> Some
                | [| "hsv:"; h; s; v; ht; st; vt |] -> DetectTarget(h,s,v,ht,st,vt) |> Target |> Some
                | z -> System.Console.WriteLine("object sensor parse error! None {0}", z); None

    override self.Dispose() = base.Dispose()    
    
    override self.Finalize() = self.Dispose()

    

    