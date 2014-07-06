namespace Trik
open System

type ButtonEventCode  = 
                    Sync    = 0
                    | Enter = 28 
                    | Up    = 103 
                    | Left  = 105 
                    | Right = 106
                    | Down  = 108
                    | Power = 116
                    | Menu  = 139


/// Button event type in a form of (Button Code, Pressed/Released) 
type ButtonEvent = ButtonEventCode * bool

type Button (deviceFilePath) as btn = 
    inherit FifoSensor<ButtonEvent>(deviceFilePath, 16, 16)
    do  btn.ParseFunc <- fun bytes offset ->  
            let evType = BitConverter.ToUInt16(bytes, offset + 8)
            let evCode = BitConverter.ToUInt16(bytes, offset + 10)
            let evValue = BitConverter.ToInt32(bytes, offset + 12)
            if evType = 1us then Some (enum<ButtonEventCode>(int evCode), (evValue = 1))
            else None