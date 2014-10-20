namespace Trik.Internals
open System
open System.IO
open System.Threading
open Trik

[<AbstractClass>]
type VideoSensor<'Parsed>(scriptPath, commandPath: string, sensorPath) = 
    inherit StringFifoSensor<VideoSensorOutput<'Parsed>>(sensorPath)
    let mutable stream = null
    let mutable commandFifo: StreamWriter = null
    let script cmd = Helpers.SendToShell <| scriptPath + " " + cmd
    let mutable videoOut = true
    let mutable isDisposed = false
    let cts = new CancellationTokenSource()
    
    member self.Start() = 
        script "start"; base.Start()

        stream <- new FileStream(commandPath, FileMode.Open, FileAccess.Write)
        commandFifo <- new StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)
        //self.Detect(DetectTarget(0,0,0,0,0,0))
        //notifier.Publish |> Async.AwaitObservable |> Async.Ignore |> Async.Start
        //notifier.OnNext <| DetectTarget(0,0,0,0,0,0)
    
    //member self.Restart() = script "restart"

    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"
    
    ///Invokes generation of the most visible color to output
    member self.Detect() = 
        if commandFifo = null then invalidOp "missing Start() before call"
        commandFifo.WriteLine("detect");commandFifo.WriteLine("detect")
 
    /// Makes the sensor detect the specified target value
    member self.SetDetectTarget(target: DetectTarget) = 
        if commandFifo = null then invalidOp "missing Start() before call"
        self.SendCommand <| target.ToString()
        self.SendCommand <| target.ToString()

    ///Sends command-string to the sensor. For TRIK internal use only
    member self.SendCommand(command: string) = commandFifo.WriteLine command

    /// Specifies video out modes. True for enable the video translation
    member self.VideoOut
        with get() = videoOut
        and set command = 
            if command <> videoOut then
                self.SendCommand <| "video_out" + if command then "1" else "0" 
                self.SendCommand <| "video_out" + if command then "1" else "0" 
                videoOut <- command
            
    override self.Dispose() = 
        if not isDisposed then
            cts.Cancel()
            commandFifo.Dispose()
            stream.Dispose()
            base.Dispose()
            self.Stop()
            isDisposed <- true