// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Trik  
open System.Reactive 
open System.Reactive.Linq

[<EntryPoint>]
let main argv = 
    let f = ref 0

    let obs = Observable.Interval(System.TimeSpan.FromSeconds(1.))
    System.Threading.Thread.Sleep(10000)
    obs.Subscribe(printfn "Initial Sequence %d")|> ignore
    //Trik.Observable.DistinctUntilChanged(obs).Subscribe(printfn "Distinct Sequence %d")|> ignore
    System.Console.Read() |> ignore
    printfn "%A" argv
    0 // return an integer exit code
