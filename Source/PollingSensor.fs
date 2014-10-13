namespace Trik.Internals

[<AbstractClass>]
type PollingSensor<'T>() = 
    let defaultRate = 50.0
    abstract Read: unit -> 'T
    member x.ToObservable(refreshRate: System.TimeSpan) = 
        Trik.Observable.Interval(refreshRate) 
        |> Observable.map (fun _ -> x.Read())
    member x.ToObservable() = x.ToObservable(System.TimeSpan.FromMilliseconds defaultRate)