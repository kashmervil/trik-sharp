namespace Trik.Internals
open System
open System.IO
open Trik

[<AbstractClass>]
type VideoSensor<'Parsed>(scriptPath, commandPath: string, sensorPath) = 
    inherit StringFifoSensor<'Parsed>(sensorPath)
    let mutable stream = null
    let mutable commandFifo: StreamWriter = null
    let script cmd = Helpers.SendToShell <| scriptPath + " " + cmd
    let mutable videoOut = true
    let mutable currentHSV = HSV()
    let mutable isDisposed = false

    member self.Start() = 
        script "start"; base.Start()

        stream <- new FileStream(commandPath, FileMode.Open, FileAccess.Write)
        commandFifo <- new StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)
    
    //member self.Restart() = script "restart"

    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"
    
    /// Starts to detect a new color by setting the most visible color from sensor to active
    member self.Detect() = 
        if commandFifo = null then invalidOp "missing Start() before call"
        commandFifo.WriteLine("detect");commandFifo.WriteLine("detect")
    
    /// Starts to detect the specified color value
    member self.Detect(hsv) = 
        if currentHSV <> hsv then
                self.SendCommand <| hsv.ToString()
                self.SendCommand <| hsv.ToString()
                lock self (fun _ -> currentHSV <- hsv)
    
    member self.SendCommand(command: string) = commandFifo.WriteLine command

    member self.VideoOut
        with get() = videoOut
        and set command = 
            if command <> videoOut then
                self.SendCommand <| "video_out" + if command then "1" else "0" 
                self.SendCommand <| "video_out" + if command then "1" else "0" 
                videoOut <- command
            
    override self.Dispose() = 
        if not isDisposed then
            commandFifo.Dispose()
            stream.Dispose()
            base.Dispose()
            self.Stop()
            isDisposed <- true