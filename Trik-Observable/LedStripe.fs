namespace Trik.Observable

open System
open Trik

type LedStripe(rc,gc,bc, gnd) =
    let r = new PowerMotor(rc)
    let g = new PowerMotor(gc)
    let b = new PowerMotor(bc)
    let gnd = new PowerMotor(gnd)
    let foreach f = f r; f g; f b; f gnd
    do gnd.SetPower 100

    interface IObserver<int*int*int> with
        member this.OnNext((r',g',b')) = 
            r.SetPower -r'
            g.SetPower -g'
            b.SetPower -b'

        member this.OnError(e) = ()
        member this.OnCompleted() = foreach (fun x -> (x:>IObserver<_>).OnCompleted())
    interface IDisposable with
        member x.Dispose() = foreach (fun x -> (x:>IDisposable).Dispose())
    

    
    