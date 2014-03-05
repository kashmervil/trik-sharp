namespace Trik.Observable
open System
open Trik

[<RequireQualifiedAccess>]
[<Flags>]
type LedColor = Green = 1 | Red = 2 | Orange = 3 | Off = 0

type Led(conf:Config.Provider.DomainTypes.Led) =
    let on = Text.Encoding.ASCII.GetBytes(string conf.On)
    let off = Text.Encoding.ASCII.GetBytes(string conf.Off)
    let green = IO.File.OpenWrite(conf.Green)
    let red = IO.File.OpenWrite(conf.Red)
    
    let setTo (c:LedColor)  =
        let inline ifFlag f = if c.HasFlag f then on else off
        green.Write(ifFlag LedColor.Green, 0, 1); green.Flush()
        red.Write(ifFlag LedColor.Red, 0, 1); red.Flush()
    do setTo LedColor.Off
    
    member x.SetColor c = setTo c

    interface IObserver<LedColor> with
        member this.OnNext(c) = setTo c 
        member this.OnError(e) = setTo LedColor.Off
        member this.OnCompleted() = setTo LedColor.Off

    interface IDisposable with
        member x.Dispose() = 
            setTo LedColor.Off 
            (green:>IDisposable).Dispose()
            (red:>IDisposable).Dispose()
    

    
    