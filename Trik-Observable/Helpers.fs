module Trik.Helpers

open System
open System.Runtime.InteropServices
open System.Reactive.Linq
open System.Collections.Generic
open System.IO

let GlobalStopwatch = new Diagnostics.Stopwatch()
GlobalStopwatch.Start()

[<Measure>]
type ms
[<Measure>]
type permil


let private I2CLockObj = new Object()

[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_init(string, int, int)
[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_SendData(int, int, int) 
[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern int private wrap_I2c_ReceiveData(int)

let isLinux = not <| Environment.OSVersion.VersionString.StartsWith "Microsoft"
let inline trikSpecific f = if isLinux then f () else ()

let Syscall_shell cmd  = 
    let args = sprintf "-c '%s'" cmd
    trikSpecific <| fun () ->
        let proc = System.Diagnostics.Process.Start("/bin/sh", args)
        printfn "Syscall: %A" cmd
        proc.WaitForExit()
        proc.ExitCode |> ignore
        //if proc.ExitCode  <> 0 then
        //    printf "Init script failed '%s'" cmd

let I2CLockCall f args : 'T = 
        if isLinux then lock I2CLockObj <| fun () -> f args
        else Unchecked.defaultof<'T>


module I2C = 
    let inline init string deviceId forced = I2CLockCall wrap_I2c_init (string, deviceId, forced)
    let inline send command data len = I2CLockCall wrap_I2c_SendData (command, data, len)  
    let inline receive (command: int) = I2CLockCall wrap_I2c_ReceiveData command

let loadIniConf path = 
    IO.File.ReadAllLines (path)
        |> Seq.choose(fun s -> 
            if s.Length > 0 then 
                let parts = 
                    s.Split ([| '=' |], StringSplitOptions.RemoveEmptyEntries) 
                    |> Array.map(fun s -> s.Trim([| ' '; '\r' |]) )
                Some(parts.[0], parts.[1]) 
            else None)
        |> dict

let fastInt32Parse (s:string) = 
    let mutable n = 0
    let start = if s.Chars 0 |> Char.IsDigit then 0 else 1
    let sign = if s.Chars 0 = '-' then -1 else 1
    let zero = int '0'
    for i = start to s.Length - 1 do 
        n <- n * 10 + int (s.Chars i) - zero
    sign * n

let inline konst c _ = c

let inline limit l u x = if u < x then u elif l > x then l else x  

let inline milliseconds x = 1<ms>*x

let inline permil min max v = 
    let v' = limit min max v
    (1000<permil> * (v' - min))/(max - min)

let defaultRefreshRate = 50.0

type PollingSensor<'T>() = 
    [<DefaultValueAttribute>]
    val mutable ReadFunc: (unit -> 'T)
    member x.Read() = x.ReadFunc()
    member x.ToObservable(refreshRate: System.TimeSpan) = 
        let rd = x.Read
        Observable.Generate((), konst true, id, x.Read, konst refreshRate)
    member x.ToObservable() = x.ToObservable(System.TimeSpan.FromMilliseconds defaultRefreshRate)

type FifoSensor<'T>(path: string, dataSize, bufSize) as sens = 
    let stream = File.Open(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    let observers = new ResizeArray<IObserver<'T> >()
    let obs = Observable.Create(fun observer -> 
        lock observers <| fun () -> observers.Add(observer) |> ignore 
        { new IDisposable with 
            member this.Dispose() = lock observers <| fun () -> observers.Remove(observer) |> ignore } )
    let obsNext (x: 'T) = lock observers <| fun () -> observers |> Seq.iter (fun obs -> obs.OnNext(x) ) 
    let bytes = Array.zeroCreate bufSize
    let mutable disposed = false
    let mutable offset = 0
    let rec reading() = async {
        if not disposed then 
            let! readCnt = stream.AsyncRead(bytes, 0, bytes.Length)
            let blocks = readCnt / dataSize
            offset <- 0
            seq {1 .. blocks} |> 
                Seq.iter (fun _ -> 
                    sens.ParseFunc bytes offset |> Option.iter obsNext
                    offset <- offset + dataSize) 
            return! reading()
    }
    do Async.Start <| reading()
    [<DefaultValue>]
    val mutable ParseFunc: (byte[] -> int -> 'T option)
    member x.ToObservable() = obs
    interface IDisposable with
        member x.Dispose() = 
            disposed <- true
            stream.Dispose()
    //member x.Read() = x.ReadFunc()
(*
type AbstractSensor<'T>(read) = 
    member x.Read() = read()
    member x.ToObservable(refreshRate: System.TimeSpan) = 
        Observable.Generate( (), konst true, id, read, konst refreshRate)
    member x.ToObservable() = x.ToObservable(System.TimeSpan.FromMilliseconds defaultRefreshRate)



[<AbstractClassAttribute>]
type UnivSensor1<'T>() = 
    member x.ToObservable(refreshRate: System.TimeSpan) = 
        let rd = x.Read
        Observable.Generate( (), konst true, id, x.Read, konst refreshRate)
    member x.ToObservable() = x.ToObservable(System.TimeSpan.FromMilliseconds defaultRefreshRate)
    *)

(*
type Sens3d (min, max, deviceFilePath)  = 
    inherit UnivSensor1<int*int*int>()
    [<Literal>]
    let event_size = 16
    [<Literal>]
    let ev_abs = 3us

    let stream = IO.File.Open(deviceFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read) 
    let mutable last = Array.zeroCreate 3
    let bytes = Array.zeroCreate event_size     

    let rd() = 
        let readCnt = stream.Read(bytes, 0, bytes.Length)
        if readCnt <> event_size then
            failwith "event reading error\n"
        else
            let evType = BitConverter.ToUInt16(bytes, 8)
            let evCode = BitConverter.ToUInt16(bytes, 10)
            let evValue = BitConverter.ToInt32(bytes, 12)
            //printfn "evType: %A" evType
            if evType = ev_abs && evCode < 3us then 
                last.[int evCode] <- limit min max evValue 
            (last.[0], last.[1], last.[2])
    override x.Read() = () *)
(* TODO (not delete)
let observableGenerate (init: 'T) (iter: 'T -> 'T) (res: 'T -> 'R) (timeSelector: 'T -> int)= 
    let subscriptions = ref (new HashSet< IObserver<'T> >())
    let thisLock = new Object()
    let stored = ref init
    let next(obs) = 
        (!subscriptions) |> Seq.iter (fun x ->  x.OnNext(obs) ) 
    let obs = 
        { new IObservable<'T> with
            member this.Subscribe(obs) =               
                lock thisLock (fun () ->
                    (!subscriptions).Add(obs) |> ignore
                    )
                { new IDisposable with 
                    member this.Dispose() = 
                        lock thisLock (fun () -> 
                            (!subscriptions).Remove(obs))  |> ignore } }
    let milis = timeSelector(!stored);

    let rec loop() = async {
        next(!stored)
        stored := iter (!stored)
        Thread.Sleep( milis )
        return! loop()
    }
    Async.Start <| loop()
    obs

let distinctUntilChanged (sq: IObservable<'T>) : IObservable<'T> = 
    let prev = ref (None : 'T option)
    Observable.filter (fun x -> 
        match !prev with 
        | Some y when x = y -> false 
        | _ -> prev := Some(x); true            
        ) sq
        *)