namespace Trik.Observable

open System
open Trik

type LedStripe(commandNumbers: int[]) =
    interface IObserver<int array> with
        member this.OnNext(data: int[]) = data |> Array.iter2 (fun x v -> Helpers.I2C.send x v 1) commandNumbers 
        member this.OnError(e) = ()
        member this.OnCompleted() = ()
    

    
    