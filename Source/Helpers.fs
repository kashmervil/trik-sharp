module Trik.Helpers

open System
open System.Runtime.InteropServices
open System.Collections.Generic
open System.IO

[<AutoOpenAttribute>]
module Measures = 
    [<Measure>] type ms
    let millisec = 1<ms>
    [<Measure>] type permil

    [<Measure>] type tick

    [<Measure>] type rad 


let SendToShell cmd = 
    let args = sprintf "-c '%s'" cmd
    //printfn "%s" args
    let proc = System.Diagnostics.Process.Start("/bin/sh", args)
    proc.WaitForExit()
    if proc.ExitCode  <> 0 then
        printf "Init script failed '%s'" cmd

let AsyncSendToShell cmd = async { SendToShell cmd }
let PostToShell cmd = AsyncSendToShell cmd |> Async.Start
let say text = PostToShell <| "espeak -v russian_test -s 100 \"" + text + "\" 2> /dev/null"
    
let playFile (file:string) = 
        PostToShell <| 
        if   file.EndsWith(".wav") then "aplay --quiet &quot;" + file + "&quot; &amp;"
        elif file.EndsWith(".mp3") then "cvlc  --quiet &quot;" + file + "&quot; &amp;"
        else invalidArg "file" "Incorrect filename"


let takeScreenshot() = PostToShell <| let date = DateTime.Now in
                                      "fbgrab trik-screenshot-"
                                      + date.ToString("yyMMdd-HHmmss.pn\g")
                                      + " 2> /dev/null"

let defaultName() = 
    let date = DateTime.Now
    "trik-cam-" + date.ToString("yyMMdd-HHmmss.jp\g")

let takePicture() = "v4l2grab -d \"/dev/video2\" -H 640 -W 480 -o " + defaultName() + " 2> /dev/null"

module I2C =
    [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_init(string, int, int)
    [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_SendData(int, int, int) 
    [<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern int private wrap_I2c_ReceiveData(int)
    
    let private I2CLockObj = new Object()

    let I2CLockCall f args : 'T = 
        lock I2CLockObj <| fun () -> f args

    let inline Init string deviceId forced = I2CLockCall wrap_I2c_init (string, deviceId, forced)
    let inline Send command data len = I2CLockCall wrap_I2c_SendData (command, data, len)  
    let inline Receive (command: int) = I2CLockCall wrap_I2c_ReceiveData command

let loadIniConf path = 
    IO.File.ReadAllLines (path)
        |> Seq.choose(fun s -> 
            if s.Length > 0 then 
                let parts = 
                    s.Split ([| '=' |], StringSplitOptions.RemoveEmptyEntries) 
                    |> Array.map(fun s -> s.Trim([| ' '; '\r' |]) )
                Some(parts.[0], parts.[1]) 
            else None)
        |> dict

let fastInt32Parse (s:string) = 
    let mutable n = 0
    let start = if s.Chars 0 |> Char.IsDigit then 0 else 1
    let sign = if s.Chars 0 = '-' then -1 else 1
    let zero = int '0'
    for i = start to s.Length - 1 do 
        n <- n * 10 + int (s.Chars i) - zero
    sign * n

/// Squishes Value between lowBound and upBound
let inline limit lowBound upBound value = if upBound < value then upBound 
                                          elif lowBound > value then lowBound 
                                          else value  

let inline milliseconds x = 1<ms>*x

let inline permil min max v = 
    let v' = limit min max v
    (1000<permil> * (v' - min))/(max - min)

//Takes H from [0..359], S & V from [0..1]. Returns R, G, B from [0..1] each
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

//Takes R, G, B from [0..1] each. Returns H from [0..359], S & V from [0..1]
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