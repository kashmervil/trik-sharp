namespace Trik.Internals
open Trik.Reactive
[<AbstractClass>]
type PollingSensor<'T>() = 
    let defaultRate = 50.0
    abstract Read: unit -> 'T
    member x.ToObservable(refreshRate: System.TimeSpan) = 
        Observable.interval(refreshRate) 
        |> Observable.map (fun _ -> x.Read())
    member x.ToObservable() = x.ToObservable(System.TimeSpan.FromMilliseconds defaultRate)