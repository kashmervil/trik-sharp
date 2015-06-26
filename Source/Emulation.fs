namespace Trik.Internals
open System
open Trik

/// Internal functionality for emulation of key pressing. Write these bytes to /dev/input/event0
module Emulations = 
    
    let private getChunk (evTime: float) (evType: uint16) (evCode: uint16) (evValue: int) = 
            [| BitConverter.GetBytes evTime; BitConverter.GetBytes evType
            ; BitConverter.GetBytes evCode; BitConverter.GetBytes evValue |] |> Array.concat
    let inline private helper (x: ButtonEventCode) isPressed =
        [|getChunk 1.0 1us (Convert.ToUInt16 x) isPressed
        ;getChunk 1.0 0us 0us 0|] |> Array.concat

    /// <summary>Serialize press-event for TRIK buttons system</summary>
    /// <param name="key">Code of used button</param>
    [<CompiledName("SendButtonPress")>]
    let sendButtonPress key = helper key 1

    /// <summary>Serialize release-event for TRIK buttons system</summary>
    /// <param name="key">Code of used button</param>
    [<CompiledName("SendButtonRelease")>]
    let sendButtonRelease key = helper key 0

    /// <summary>Serialize Press and Release (Click) event for TRIK buttons system</summary>
    /// <param name="key">Code of used button</param>
    [<CompiledName("SendButtonClick")>]
    let sendButtonClick (key: ButtonEventCode) = [| sendButtonPress key; sendButtonRelease key|] |> Array.concat

    /// <summary>Serialize Sync for TRIK buttons system</summary>
    [<CompiledName("SendButtonSync")>]
    let sendButtonSync = getChunk 1.0 0us 0us 0


