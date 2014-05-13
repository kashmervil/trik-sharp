namespace Trik
open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
open Trik.Helpers

type Button_Event_Code  = 
                    Sync    = 0
                    | Enter = 28 
                    | Up    = 103 
                    | Left  = 105 
                    | Right = 106
                    | Down  = 108
                    | Power = 116
                    | Menu  = 139

type Button_Event = Button_Event_Code * bool

type Button (deviceFilePath) as btn = 
    inherit FifoSensor<Button_Event>(deviceFilePath, 16, 16)
    do  btn.ParseFunc <- fun bytes offset ->  
            let evType = BitConverter.ToUInt16(bytes, offset + 8)
            let evCode = BitConverter.ToUInt16(bytes, offset + 10)
            let evValue = BitConverter.ToInt32(bytes, offset + 12)
            if evType = 1us then Some (enum<Button_Event_Code>(int evCode), (evValue = 1) )
            else None