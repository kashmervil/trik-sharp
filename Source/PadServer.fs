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

    let obs_src = new Event<PadEvent>()
    let obs = obs_src.Publish
    let obsNext = obs_src.Trigger
    let mutable working = false
    let mutable request_accumulator = ""
    let messageBuf = Array.create 1024 <| byte 0
    let handleRequst (req:String) = 
        match req.Split([| ' ' |]) with
        | [| "wheel"; x |] -> PadEvent.Wheel(Int32.Parse(x) ) |> obsNext 
        | [| "pad"; n; "up" |] -> PadEvent.Pad(Int32.Parse(n), None ) |> obsNext 
        | [| "pad"; n; x; y |] -> PadEvent.Pad(Int32.Parse(n), Some (Int32.Parse(x), Int32.Parse(y) ) ) |> obsNext 
        | [| "btn"; n; "down" |] -> PadEvent.Button(Int32.Parse(n) ) |> obsNext 
        | _ -> printfn "PadServer unresolved: %A" req

    let rec clientLoop(client: TcpClient) = async {
            if client.Connected then 
                let! count = client.GetStream().AsyncRead(messageBuf, 0, messageBuf.Length)
                let msg = Encoding.ASCII.GetString(messageBuf, 0, count)   
                msg.TrimEnd [| '\r'; '\n' |] 
                |> handleRequst 
                return! clientLoop(client)
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

            