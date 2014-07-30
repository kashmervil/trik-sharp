namespace Trik
open System

///Type representing on-board light emitting diode
type Led(deviceFilePath: string) =
    let on = [| byte 49|]
    let off =[| byte 48|]
    let green = IO.File.OpenWrite(deviceFilePath + "/led_green/brightness")
    let red = IO.File.OpenWrite(deviceFilePath + "/led_red/brightness")
    
    //do led.Color <- LedColor.Off
    
    let mutable color = LedColor.Off
    ///Led has three colors Green, Red, Orange 
    member self.Color 
        with get() = color
        and set (c: LedColor) = 
            let inline ifFlag f = (if c.HasFlag f then on else off), 0, 1
            green.Write(ifFlag LedColor.Green); green.Flush()
            red.Write(ifFlag LedColor.Red); red.Flush()
            color <- c
    ///Powers off the led
    member self.PowerOff() = self.Color <- LedColor.Off

    interface IObserver<LedColor> with
        member self.OnNext(c) = self.Color <- c
        member self.OnError(e) = self.PowerOff()
        member self.OnCompleted() = self.PowerOff()

    interface IDisposable with
        member self.Dispose() = 
            self.PowerOff() 
            (green:>IDisposable).Dispose()
            (red:>IDisposable).Dispose()
    

    
    