module PowerMotor

open System
open Extern
open Config


let isWin = (Environment.OSVersion.VersionString = "Microsoft Windows NT 6.2.9200.0")


type PowerMotor(i2cCommandNumber) =
    let mutable inner = 0
    //let limit l u (x: int) = Math.Min(u, Math.Max (l, x)) 
    interface IObserver<int> with
        member this.OnNext(data) = 
            if inner > 10 then 
                inner <- 0
                printfn "%A" data
            else 
                inner <- inner + 1
            if (not isWin) then 
                Extern.send i2cCommandNumber data 1
            else () 
        member this.OnError(e) = ()
        member this.OnCompleted() = ()
    
    