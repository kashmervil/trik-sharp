module Trik.Helpers

open System
open System.Runtime.InteropServices

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

let inline konst c _ = c

let inline limit l u x = if u < x then u elif l > x then l else x  

let inline milliseconds x = 1<ms>*x

let inline permil min max v = 
    let v' = limit min max v
    (1000<permil> * (v' - min))/(max - min)


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