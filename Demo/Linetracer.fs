module Linetracer

open Trik
open System
open System.IO
open System.Collections
open System.Diagnostics

let logFifoPath = @"/tmp/dsp-detector.out.fifo"
let cmdFifoPath = @"/tmp/dsp-detector.in.fifo"

let speed = 100;
let stopK = 1;
let PK = 0.42;
let IK = 0.006;
let DK = -0.009;
let encC = 1.0 / (334.0 * 34.0); //1 : (num of points of encoder wheel * reductor ratio)
let max_fifo_input_size = 4000

type MutableBuf<'T> (size) = 
    let buf = Array.zeroCreate size
    let mutable l = 0
    member x.IndexOf b = 
        let mutable found = -1
        for i = 0 to l - 1 do 
            if buf.[i] = b then found <- i
        found
    member x.Append (nbuf:byte[], cnt:int) = 
        for i in 0 .. (cnt - 1) do 
            buf.[l + i] <- nbuf.[i]
        l <- l + cnt
    member x.CopyFrom(from, cnt) = 
        Array.ConstrainedCopy(from, 0, buf, 0, cnt)
        l <- cnt
    member x.Buf = buf
    member x.Length 
        with get() = l
        and set(v) = l <- v
    override x.ToString() = Text.Encoding.ASCII.GetString(buf, 0, l)
    
type LogFifo(path:string) = //, _lineTargetDataParsed, _lineColorDataParsed) = 
    //let fd = File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    let sr = new StreamReader(path)
    //let rest = new MutableBuf<byte> (max_fifo_input_size * 3)
    //let mutable rest = Array.zeroCreate (max_fifo_input_size * 3), 0
    //let rest = new Text.StringBuilder ""
    let mutable loopDone = false
    let buf = Array.zeroCreate max_fifo_input_size
    let lineTargetDataParsed = new Event<_>()
    let lineColorDataParsed = new Event<_>()
    let eol = 
        let buf = Text.Encoding.ASCII.GetBytes("\n")
        buf.[0]
        //byte 10
    let checkLines (lines:string[]) last = 
        let mutable i = last
        let mutable wasLoc = false
        let mutable wasHsv = false
        while (not (wasHsv && wasLoc) ) && i >= 0 do
            let logStruct = lines.[i].Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)
            //printfn "%s" logStruct.[0]
            if not wasLoc && logStruct.[0] = "loc:" then 
                let x     = Helpers.fastInt32Parse logStruct.[1]
                let angle = Helpers.fastInt32Parse logStruct.[2]
                let mass  = Helpers.fastInt32Parse logStruct.[3]
                wasLoc <- true
                lineTargetDataParsed.Trigger(x, angle, mass)
                //printfn "ltparsed"
            elif not wasHsv && logStruct.[0] = "hsv:" then
                let hue    = Helpers.fastInt32Parse logStruct.[1]
                let hueTol = Helpers.fastInt32Parse logStruct.[2]
                let sat    = Helpers.fastInt32Parse logStruct.[3]
                let satTol = Helpers.fastInt32Parse logStruct.[4]
                let _val   = Helpers.fastInt32Parse logStruct.[5]
                let valTol = Helpers.fastInt32Parse logStruct.[6]
                wasHsv <- true
                lineColorDataParsed.Trigger(hue, hueTol, sat, satTol, _val, valTol)
            else ()
            i <- i - 1
    let rec loop() = 
        let ln = sr.ReadLine()
        eprintfn "%s" ln
        checkLines [| ln |] 0
        //printfn "LogFifoLoop"
        (*let cnt = fd.Read(buf, 0, max_fifo_input_size)
        rest.Append(buf, cnt)
        rest.Append([| eol; eol |], 2)
        eprintfn "%A %A %A\n" eol cnt rest.Length
        if rest.IndexOf(eol) <> -1 then
            let mutable cnt = 0
            for i = 0 to rest.Length - 1 do 
                if rest.Buf.[i] = eol then cnt <- cnt + 1 
            eprintfn "cnt: %d\n" cnt
            let mutable rest_str = rest.ToString()
            
            eprintfn "rest_str: %s\n" rest_str

            let lines = rest_str.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries)
            rest_str <- lines.[lines.Length - 1]
            let rest_str_buf = Text.Encoding.ASCII.GetBytes(rest_str)
            rest.CopyFrom (rest_str_buf, rest_str.Length)
            checkLines lines (lines.Length - 2)*)
        if not loopDone then loop() 
    do Async.Start <| async { loop() } 
    member x.LineTargetDataParsed = lineTargetDataParsed.Publish
    member x.LineColorDataParsed = lineColorDataParsed.Publish

type CmdFifo(path:string) = 
    let fd = new IO.FileStream(path, FileMode.Truncate)
    let mutable dh = null
    let mutable th = null
    member x.Write(cmd:string) = 
        let buf = Text.Encoding.ASCII.GetBytes(cmd + "\n")
        fd.Write(buf, 0, buf.Length)
        fd.Flush()
    member x.Detect (logFifo:LogFifo) f = 
        //f(246, 134, 9, 9, 31, 31)
        f(223, 111, 18, 18, 9, 9)

        //th <- new Handler<_>(fun _ _ -> () )
        //let tm = new System.Timers.Timer(500)
        (*
        h <- new Handler<_> (fun _ a -> 
            eprintfn "x.Detect Handler"
            logFifo.LineColorDataParsed.RemoveHandler h
            f a )
        logFifo.LineColorDataParsed.AddHandler(h)
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        x.Write("detect")
        *)
        eprintfn "x.Detect"


    interface IDisposable with
        member x.Dispose() = fd.Dispose()


type MotorControler (motor: Trik.PowerMotor, enc: Encoder, sign) = 
    [<Literal>]
    let max_ppms = 23.833f
    [<DefaultValue>]
    val mutable ActualSpeed : int
    [<DefaultValue>]
    val mutable PowerAddition : int
    [<DefaultValue>]
    val mutable PowerBase : int
    let mutable currentSpeed = 0.0f
    let mutable encPoints_prev = 0.0f
    let sw = new Stopwatch()
    do sw.Start()
    let mutable time_prev = sw.ElapsedMilliseconds
    let countSpeed() = 
        let encPoints = enc.Read()
        let time = sw.ElapsedMilliseconds
        let period = time - time_prev |> float32
        let speed = (encPoints - encPoints_prev) * 100.0f / (period * max_ppms)

        if currentSpeed <> speed then ()

        currentSpeed <- speed
        time_prev <- time
        encPoints_prev <- encPoints
        speed
    member x.doStep() = 
        x.ActualSpeed <- x.PowerBase + x.PowerAddition 
        motor.SetPower(sign * x.ActualSpeed)    

type Linetracer (model:Model) = 
    let cmd_fifo = new CmdFifo(cmdFifoPath)
    let log_fifo = new LogFifo(logFifoPath)
    //do eprintfn "stop here"
    //do System.Console.ReadKey |> ignore
    let localConfPath = "ltc.ini"
    let conf = Helpers.loadIniConf localConfPath

    let min_mass = conf.["min_mass"] |> Helpers.fastInt32Parse
    let power_base = conf.["power_base"] |> Helpers.fastInt32Parse
    let motor_sign = conf.["motor_sign"] |> Helpers.fastInt32Parse
    let rl_sign = conf.["rl_sign"] |> Helpers.fastInt32Parse
    let div_coefL = conf.["div_coefL"] |> Double.Parse
    let div_coefR = conf.["div_coefR"] |> Double.Parse
    let on_lost_coef = conf.["on_lost_coef"] |> Double.Parse
    do eprintfn "conf: powbase: %s" conf.["power_base"]

    let motorL = new MotorControler(model.Motor.["JM1"], model.Encoder.["JB2"], motor_sign, PowerBase = power_base)
    let motorR = new MotorControler(model.Motor.["JM3"], model.Encoder.["JB3"], motor_sign * -1, PowerBase = power_base)
    let mutable last_pow_add = 0
    let startAutoMode() = 
        let unsub = 
            log_fifo.LineTargetDataParsed
            //|> Observable.subscribe(fun (x, angle, mass) -> eprintfn "lt: %d %d" x mass )
            //|> Observable.filter(fun (x, angle, mass) -> mass > min_mass )
            |> Observable.add (fun (x, angle, mass) -> 
                if mass = 0 then
                    motorL.PowerAddition <- int <| float(last_pow_add) * div_coefL * on_lost_coef
                    motorR.PowerAddition <- int <| float(-last_pow_add) * div_coefR * on_lost_coef
                else 
                    last_pow_add <- x * rl_sign
                    motorL.PowerAddition <- int <| float(last_pow_add) * div_coefL
                    motorR.PowerAddition <- int <| float(-last_pow_add) * div_coefR
                //eprintfn "lt: %d %d" motorL.PowerAddition motorR.PowerAddition
                motorL.doStep()
                motorR.doStep()
                () ) 
                
        ()
    member x.Run() = 
        eprintfn "Linetracer.Run"
        cmd_fifo.Detect log_fifo <| fun (hue, hueTol, sat, satTol, _val, valTol) ->
            //printfn "detected: %d %d " hue hueTol
            printfn "detected"
            cmd_fifo.Write <| sprintf "hsv %d %d %d %d %d %d" hue hueTol sat satTol _val valTol
            startAutoMode()
        eprintfn "Ready (any key to finish)"
        System.Console.ReadKey() |> ignore