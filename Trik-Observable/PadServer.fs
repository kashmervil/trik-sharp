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
    let server = async {
        let listener = new TcpListener(IPAddress.Any, padPortVal)
        listener.Start()
        printfn "Listening on %d..." padPortVal
        let rec loop() = async {
            let client = listener.AcceptTcpClient()
            let rec clientLoop() = async {
                let request = 
                    let buf = Array.create client.ReceiveBufferSize <| byte 0
                    let count = client.GetStream().Read(buf, 0, buf.Length) 
                    Encoding.ASCII.GetString(buf, 0, count)   
                let sempty s = (s = null || s = "")
                request.Split([| '\n' |]) |> Array.filter (not << sempty) |> Array.iter (fun req -> 
                    obsNext <| 
                        match req.Split([| ' ' |]) |> Array.toList |> List.filter (not << sempty) with
                        | ["wheel"; x ] -> PadEvent.Wheel(Int32.Parse(x) )
                        | ["pad"; n; "up"] -> PadEvent.Pad(Int32.Parse(n), None )
                        | ["pad"; n; x; y] -> PadEvent.Pad(Int32.Parse(n), Some (Int32.Parse(x), Int32.Parse(y) ) )
                        | ["btn"; n; "down"] -> PadEvent.Button(Int32.Parse(n) )
                        | _ -> failwithf "error in request %A" req)
                return! clientLoop() 
            }
            Async.Start(clientLoop() )
            return! loop() 
        }
        Async.Start(loop() )
    }
    member val Observable = obs
    member this.Start() = Async.Start server