namespace Trik.Internals
open System
open System.IO
open System.Threading
open Trik

[<AbstractClass>]
type VideoSensor<'Parsed>(scriptPath, commandPath: string, sensorPath) as vs = 
    inherit StringFifoSensor<'Parsed>(sensorPath)
    let mutable stream = null
    let mutable commandFifo: StreamWriter = null
    let script cmd = Helpers.SendToShell <| scriptPath + " " + cmd
    let mutable videoOut = true
    let mutable currentTarget = DetectTarget()
    let notifier = new Notifier<DetectTarget>()
    let mutable isDisposed = false
    let targetUpdater = notifier.Publish.Subscribe(fun x -> lock vs (fun _ -> currentTarget <- x))
    let cts = new CancellationTokenSource()

    member self.Start() = 
        script "start"; base.Start()

        stream <- new FileStream(commandPath, FileMode.Open, FileAccess.Write)
        commandFifo <- new StreamWriter(stream, Text.Encoding.UTF8, AutoFlush = true)
    
    //member self.Restart() = script "restart"

    member self.Stop() = base.Stop(); commandFifo.Close(); script "stop"
    
    /// Starts an asynchronous computation which detects a new target by setting the most visible color from sensor to active
    member self.AsyncDetect() = 
        if commandFifo = null then invalidOp "missing Start() before call"
        commandFifo.WriteLine("detect");commandFifo.WriteLine("detect")
        Async.AwaitObservable notifier.Publish

    /// Makes the sensor detect a new target by setting the most visible color from sensor to active and returns 
    member self.Detect() = Async.RunSynchronously(self.AsyncDetect(), cancellationToken = cts.Token)
    
    /// Makes the sensor detect the specified target value
    member self.Detect(target) = 
        if commandFifo = null then invalidOp "missing Start() before call"
        self.SendCommand <| target.ToString()
        self.SendCommand <| target.ToString()
        notifier.OnNext target

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
    
    member self.CurrentTarget = currentTarget
            
    override self.Dispose() = 
        if not isDisposed then
            cts.Cancel()
            targetUpdater.Dispose()
            notifier.OnCompleted()
            commandFifo.Dispose()
            stream.Dispose()
            base.Dispose()
            self.Stop()
            isDisposed <- true