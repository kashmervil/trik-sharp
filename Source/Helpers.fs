namespace Trik.Helpers
open System
open System.Collections.Generic
open System.Runtime.InteropServices


/// Tools for communicating with Linux shell
module Shell =
    /// <summary>Sends command to Linux shell. Synchronously waits for its completion </summary>
    /// <param name="a">string containing Shell command</param>
    [<CompiledName("Send")>]
    let send cmd = 
        let args = sprintf "-c '%s'" cmd
        //printfn "%s" args
        let proc = System.Diagnostics.Process.Start("/bin/sh", args)
        proc.WaitForExit()
        if proc.ExitCode  <> 0 then
            printf "Init script failed '%s'" cmd

    /// <summary>Sends async command to Linux shell </summary>
    /// <param name="a">string containing Shell command </param>
    /// <returns> Async with corresponding shell command </returns>
    [<CompiledName("AsyncSend")>]
    let asyncSend cmd = async { send cmd }

    /// <summary>Asynchronously sends command to Linux shell, doesn't wait for its completion </summary>
    /// <param name="a">string containing Shell command</param>
    [<CompiledName("Post")>]
    let post cmd = asyncSend cmd |> Async.Start

/// Tools for working with photo and audio files
[<AbstractClass; Sealed>]
type Media =
    /// <summary>Takes screenshot from on-board TRIK display 320x240. Doesn't wait for completion </summary>
    /// <param name="name">String containing screenshot name</param>
    static member TakeScreenshot(name) = Shell.post <| "fbgrab " + name + " 2> /dev/null"

    /// <summary>Takes screenshot from on-board TRIK display 320x240. Waits for completion</summary>
    /// <returns>String containing screenshot name</returns>
    static member TakeScreenshot() = 
        let date = DateTime.UtcNow
        let name = "fbgrab trik-screenshot-" + date.ToString("yyMMdd-HHmmss.pn\g")
        Shell.send <| "fbgrab " + name + " 2> /dev/null"
        name

    /// <summary>Takes picture from camera. Doesn't wait for completion </summary>
    /// <param name="name">String containing picture name</param>
    static member TakePicture(photoName) = Shell.post <| "v4l2grab -d \"/dev/video2\" -H 640 -W 480 -o " + photoName + " 2> /dev/null"

    /// <summary>Uses TRIK speech generator to ACTUALLY SAY the passed text</summary>
    /// <param name="a">string containing text to be said</param>    
    static member Say text = Shell.post <| "espeak -v russian_test -s 100 \"" + text + "\" 2> /dev/null"

    /// <summary>Plays audio file. Supported formats are mp3 and wav</summary>
    /// <param name="a">string containing path to audiofile</param>
    static member PlayFile (file:string) = 
            Shell.post <| 
            if   file.EndsWith(".wav") then "aplay --quiet &quot;" + file + "&quot; &amp;"
            elif file.EndsWith(".mp3") then "cvlc  --quiet &quot;" + file + "&quot; &amp;"
            else invalidArg "file" "Incorrect file format"

/// Tools for working with I2C bus from .NET
module I2C =
    module private M =
        [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
        extern void private wrap_I2c_init(string, int, int)
        [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
        extern void private wrap_I2c_SendData(int, int, int) 
        [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
        extern int private wrap_I2c_ReceiveData(int)
    
    let private I2CLockObj = new Object()

    let inline private I2CLockCall f args = 
        lock I2CLockObj <| fun () -> f args

    /// <summary>Initializes I2C bus </summary>
    [<CompiledName("Init")>]
    let init (string, deviceId, forced) = I2CLockCall M.wrap_I2c_init (string, deviceId, forced)

    /// <summary>Send message to I2C bus </summary>
    [<CompiledName("Send")>]
    let send (command, data, len) = I2CLockCall M.wrap_I2c_SendData (command, data, len)  

    /// <summary>Receives data from I2C bus </summary>
    [<CompiledName("Receive")>]
    let receive (command: int) = I2CLockCall M.wrap_I2c_ReceiveData command

/// Small helpers for calculation routine
module Calculations =
    /// <summary>Takes H from [0..359], S & V from [0..1]. Returns R, G, B from [0..1] each </summary>
    /// <param name="(h,s,v)">Color in RGB space</param>
    /// <returns>The same color representation in RGB</returns>
    let HSVtoRGB (h, s, v) =
        if s = 0.0 then (v, v, v) 
        else
            let hs = h / 60.0
            let i = floor (hs)
            let f = hs - i
            let p = v * ( 1.0 - s )
            let q = v * ( 1.0 - s * f )
            let t = v * ( 1.0 - s * ( 1.0 - f ))
            match int i with
                | 0 -> (v, t, p)
                | 1 -> (q, v, p)
                | 2 -> (p, v, t)
                | 3 -> (p, q, v)
                | 4 -> (t, p, v)
                | _ -> (v, p, q)

    /// <summary>Takes R, G, B from [0..1] each. Returns H from [0..359], S & V from [0..1]</summary>
    /// <param name="(r,g,b)">Color in RGB space</param>
    /// <returns>The same color representation in HSV</returns>
    let RGBtoHSV (r, g, b) =
        let (m : float) = min r (min g b)
        let (M : float) = max r (max g b)
        let delta = M - m
        let posh (h : float) = if h < 0.0 then h + 360.0 else h
        let deltaf (f : float) (s : float) = (f - s) / delta
        if M = 0.0 then (-1.0, 0.0, M) 
        else
            let s = (M - m) / M
            if r = M then (posh(60.0 * (deltaf g b)), s, M)
            elif g = M then (posh(60.0 * (2.0 + (deltaf b r))), s, M)
            else (posh(60.0 * (4.0 + (deltaf r g))), s, M)

    /// <summary>Small parser for getting int from string without too many checks. !!! WORKS CORRECTRLY ONLY ON INT NUMBERS</summary>
    /// <param name="s">string representation of int number</param>
    /// <returns> produced int value</returns>
    [<CompiledName("UnsafeInt32Parse")>]
    let internal unsafeInt32Parse (s:string) = 
        let mutable n = 0
        let start = if Char.IsDigit s.[0] then 0 else 1
        let sign = if s.Chars 0 = '-' then -1 else 1
        let zero = int '0'
        for i = start to s.Length - 1 do 
            n <- n * 10 + int s.[i] - zero
        sign * n

    /// <summary>Squishes Value between lowBound and upBound. Returns value if low < value < up else returns corresponding limit</summary>
    /// <param name="upBound">The lower limit of number</param>
    /// <param name="lowBound">The lower limit of number</param>
    /// <param name="value">the initial value</param>
    /// <returns> produced squished value</returns>
    [<CompiledName("Limit")>]
    let inline limit lowBound upBound value = if upBound < value then upBound 
                                              elif lowBound > value then lowBound 
                                              else value  
    /// Creates usual writable Dictionary
    /// <summary>Creates usual writable Dictionary from source then upcasts it to IDictionary</summary>
    /// <param name="s">Array of (key, value) tuples</param>
    /// <returns> produced Dictionary</returns>
    [<CompiledName("CreateWritableDictionary")>]
    let internal writableDict xs = 
        let d = new Dictionary<_,_>()
        Array.iter d.Add xs
        d :> IDictionary<_,_>