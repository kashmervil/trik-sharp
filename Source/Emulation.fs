namespace Trik.Internals
open System
open Trik
module Emulations = 
    
    let private getChunk (evTime: float) (evType: uint16) (evCode: uint16) (evValue: int) = 
            [| BitConverter.GetBytes evTime; BitConverter.GetBytes evType
            ; BitConverter.GetBytes evCode; BitConverter.GetBytes evValue |] |> Array.concat
    let private helper (x: ButtonEventCode) isPressed =
        [|getChunk 1.0 1us (Convert.ToUInt16 x) isPressed
        ;getChunk 1.0 0us 0us 0|] |> Array.concat

    let buttonPress x = helper x 1
    let buttonRelease x = helper x 0
    let buttonClick (b: ButtonEventCode) = [| buttonPress b; buttonRelease b|] |> Array.concat
    let buttonSync = getChunk 1.0 0us 0us 0


