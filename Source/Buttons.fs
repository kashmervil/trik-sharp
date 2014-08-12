namespace Trik
open System

type ButtonPad (deviceFilePath) = 
    inherit BinaryFifoSensor<ButtonEvent>(deviceFilePath, 16, 1024)

    static let () = Enum.GetValues(typeof<ButtonEventCode>) 
                    |> Seq.cast<ButtonEventCode> 
                    |> Seq.tryFind  (fun x -> int x  < 0  || 255 < int x) 
                    |> Option.iter (fun x -> invalidArg (x.ToString()) "Button code out of bounds 0..255")

    static let state = new Collections.BitArray(256)
    
    override self.Parse (bytes, offset) =
            let evTime  = BitConverter.ToDouble(bytes, offset)
            let evType  = BitConverter.ToUInt16(bytes, offset + 8)
            let evCode  = BitConverter.ToUInt16(bytes, offset + 10)
            let evValue = BitConverter.ToInt32(bytes, offset + 12)
            if evType = 1us then 
                state.Set(int evCode, (evValue = 1))
                Some <| ButtonEvent(evCode, (evValue = 1), evTime)
            else None