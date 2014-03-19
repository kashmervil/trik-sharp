namespace Trik
open System
open System.Net
open System.Net.Sockets
open System.Reactive.Linq
open System.Text
open System.Collections.Generic


[<RequireQualifiedAccess>]
type PadEvent = 
    | Pad of int * ( int * int ) option
    | Button of int
    | Wheel of int
    | Stop

type PadServer(?port) =
    let padPortVal = defaultArg port 4444
    let observers = new HashSet<IObserver<PadEvent> >()
    let obs = Observable.Create(fun observer -> 
        observers.Add(observer) |> ignore; 
        { new IDisposable with 
            member this.Dispose() = () } )
    let obsNext (x:PadEvent) = observers |> Seq.iter (fun obs -> obs.OnNext(x) ) 

    let mutable working = false
    let mutable request_accumulator = ""

    let handleRequst (req:String) = 
        match req.TrimEnd([| '\r' |]).Split([| ' ' |]) |> Array.filter (fun s -> s.Length > 0) with
        | [| "wheel"; x |] -> PadEvent.Wheel(Int32.Parse(x) ) |> obsNext 
        | [| "pad"; n; "up" |] -> PadEvent.Pad(Int32.Parse(n), None ) |> obsNext 
        | [| "pad"; n; x; y |] -> PadEvent.Pad(Int32.Parse(n), Some (Int32.Parse(x), Int32.Parse(y) ) ) |> obsNext 
        | [| "btn"; n; "down" |] -> PadEvent.Button(Int32.Parse(n) ) |> obsNext 
        | _ -> printfn "PadServer unresolved: %A" req
    let getMessage (client:TcpClient) = 
        let buf = Array.create 1024 <| byte 0
        let mutable msg = ""
        let mutable isDone = false
        while not isDone do 
            let count = client.GetStream().Read(buf, 0, buf.Length)
            msg <- msg + Encoding.ASCII.GetString(buf, 0, count)   
            if count = 0 || msg.IndexOf('\n') >= 0 then isDone <- true else ()
        msg.TrimEnd [| '\r'; '\n' |] 
    let rec clientLoop(client: TcpClient) = async {
        let isDone = ref false
        while not !isDone do 
            if not client.Connected then isDone := true
            else client |> getMessage |> handleRequst 
    }
    let server = async {
        let listener = new TcpListener(IPAddress.Any, padPortVal)
        listener.Start()
        printfn "Listening  now on %d..." padPortVal
        let rec loop() = async {
            let client = listener.AcceptTcpClient()
            if not working then () else Async.Start(clientLoop client)
            return! loop() 
        }
        Async.Start(loop() )
    }
    do working <- true
    do Async.Start server
    let btns = obs |> Observable.choose (function PadEvent.Button(x) -> Some(x) | _ -> None)
    let pads = obs |> Observable.choose (function PadEvent.Pad(p, c) -> Some(p, c) | _ -> None)
    member val Observable = obs
    member val Buttons = btns
    member val Pads = pads
    interface IDisposable with
        member x.Dispose() = 
            working <- false

            