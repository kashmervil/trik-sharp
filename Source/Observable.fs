namespace Trik
open System
[<RequireQualifiedAccessAttribute>]
module Observable =   

    let Create(subscription) = { new IObservable<'T> with
                                    member self.Subscribe observer = subscription observer}

    let DistinctUntilChanged (sequence: IObservable<'T>  when 'T: comparison) = 
        let prev = ref None
        let func x = if (!prev).IsNone || (!prev).Value <> x 
                        then prev := Some x; !prev
                        else None
        Observable.choose func sequence
    
    let Interval(timeSpan: System.TimeSpan) = 
        let running = ref true
        let tick = 
            let r = ref 0
            fun () -> r:= !r + 1; !r

        let observers = new ResizeArray<IObserver<int>>()
        let observable = {
            new IObservable<int> with
                member self.Subscribe observer = 
                    lock observers <| fun () -> observers.Add(observer)
                    { new IDisposable with 
                        member self.Dispose() = lock observers <| fun () -> observers.Remove(observer) |> ignore 
                    }
            interface IDisposable with
                member self.Dispose() = 
                    running := false
                    observers.RemoveAll(fun _ -> true) |> ignore
                    ()
            }

        let obsNext x = lock observers <| fun () -> observers |> Seq.iter (fun obs -> obs.OnNext(x)) 
        let rec triggering() = async {
                             if !running then
                                 obsNext(tick())
                                 System.Threading.Thread.Sleep(int timeSpan.Ticks/10900)
                                 return! triggering()
                             }
        triggering() |> Async.Start
        observable

