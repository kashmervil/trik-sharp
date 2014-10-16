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
            | Ports.VideoSource.USB -> "/etc/init.d/line-sensor.sh"
            | _                     -> "/etc/init.d/line-sensor-ov7670.sh"

        new LineSensor(script, "/run/line-sensor.in.fifo", "/run/line-sensor.out.fifo")

    override self.Parse text =
            let parsedLines = text.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) 
            match parsedLines with
                | [| "loc:"; x; y; z |] -> Some (LineLocation(x, y, z))
                | [| "hsv:"; h; s; v; ht; st; vt |] -> base.Detect(DetectTarget(h,s,v,ht,st,vt))
                                                       None
                | z -> printfn "object sensor parse error! None %A" z; None
   
    override self.Dispose() = base.Dispose()

    override self.Finalize() = self.Dispose()