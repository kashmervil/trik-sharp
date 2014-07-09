namespace Trik
open System
open System.Reactive.Linq

type ButtonEventCode  = 
     | Sync  = 0
     | Enter = 28 
     | Up    = 103 
     | Left  = 105 
     | Right = 106
     | Down  = 108
     | Power = 116
     | Menu  = 139
      
/// Button event type in a form of (Button Code, Pressed/Released) 
type ButtonEvent = ButtonEventCode * bool * double

type ButtonsPad (deviceFilePath) = 
    static let () = Enum.GetValues(typeof<ButtonEventCode>) 
                    |> Seq.cast<ButtonEventCode> 
                    |> Seq.tryFind  (fun x -> int x  < 0  || 255 < int x) 
                    |> Option.iter (fun x -> invalidArg (x.ToString()) "Button code out of bounds 0..255")

    static let state = new Collections.BitArray(256)
    let maxEventSize = 1024
    let usualEventSize = 16
    
    let stream = IO.File.Open(deviceFilePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
    
    let observers = new ResizeArray<IObserver<ButtonEvent>>()
    let obsNext x = lock observers <| fun () -> observers |> Seq.iter (fun obs -> obs.OnNext x ) 
    let bytes = Array.zeroCreate maxEventSize
    let bytesBlocking = Array.zeroCreate maxEventSize
    
    let mutable continueReading = false
    let mutable offset = 0
    
    let ParseFunc = fun bytes offset ->
            let evTime  = BitConverter.ToDouble(bytes, offset)
            let evType  = BitConverter.ToUInt16(bytes, offset + 8)
            let evCode  = BitConverter.ToUInt16(bytes, offset + 10)
            let evValue = BitConverter.ToInt32(bytes, offset + 12)
            
            if evType = 1us then 
                state.Set(int evCode, (evValue = 1))
                Some (enum<ButtonEventCode>(int evCode), (evValue = 1), evTime)
  
            else None


    let rec reading() = async {
        if continueReading then 
            let! readCnt = stream.AsyncRead(bytes, 0, bytes.Length)
            let blocks = readCnt / usualEventSize
            offset <- 0
            for _ in 1 .. blocks do  
                ParseFunc bytes offset |> Option.iter obsNext
                offset <- offset + usualEventSize 
            return! reading()
        }   

    member x.BlockingRead() = 
        let cnt = stream.Read(bytesBlocking, offset, maxEventSize)
        ParseFunc bytesBlocking offset
    member x.ToObservable() = 
        continueReading <- true 
        Async.Start <| reading()
        { new IObservable<ButtonEvent> with
            member x.Subscribe observer =  
                lock observers <| fun () -> observers.Add(observer) 
                { new IDisposable with 
                    member this.Dispose() = 
                        lock observers <| fun () -> observers.Remove(observer) |> ignore 
                }
        }
        
    interface IDisposable with
        member x.Dispose() = 
            continueReading <- false
            stream.Dispose()
    
    ///Indicates access to specified button since last WasPressed(button) method call
    ///DON'T FORGET TO START BUTTON EVENT LISTENING (e.g ToObservable() |> ignore)
    member x.WasPressed(button) = ()
    //member 
              //              let result = buttonTable.[button]
                //            if result then lock lockObj (fun () -> buttonTable.[button] <- false)
                  //          result
                             
    //member x.Reset() = keys |> Array.iter (fun k -> buttonTable.[k] <- false)
    
        //let obsNext x = lock observers <| fun () -> observers |> Seq.iter (fun obs -> obs.OnNext(x) ) 



