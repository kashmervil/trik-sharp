namespace Trik.Observable

open System
open Trik

type LED(commandNumbers: int[]) =
    let mutable inner = 0
    interface IObserver<int array> with
        member this.OnNext(data: int[]) = 
            data |> Array.iter2 (fun x v -> Helpers.I2C.send x v 1) commandNumbers 
        member this.OnError(e) = ()
        member this.OnCompleted() = ()
    

    
    