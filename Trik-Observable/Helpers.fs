module Trik.Helpers

open System
open System.Runtime.InteropServices

let private I2CLockObj = new Object()

[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_init(string, int, int)
[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_SendData(int, int, int) 
[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern int private wrap_I2c_ReceiveData(int)

let isLinux = not <| Environment.OSVersion.VersionString.StartsWith "Microsoft"
let inline trikSpecific f = if isLinux then f () else ()



let Syscall_shell cmd  = 
    let args = sprintf "-c '%s'" cmd
    trikSpecific <| fun () ->
        let proc = System.Diagnostics.Process.Start("/bin/sh", args)
        proc.WaitForExit()
        if proc.ExitCode  <> 0 then
            printf "Init script failed '%s'" cmd



let I2CLockCall f args : 'T = 
        if isLinux then lock I2CLockObj <| fun () -> f args
        else Unchecked.defaultof<'T>


module I2C = 
    let inline init string deviceId forced = I2CLockCall wrap_I2c_init (string, deviceId, forced)
    let inline send command data len = //I2CLockCall wrap_I2c_SendData (command, data, len)
        Syscall_shell <| sprintf "i2cset -y 2 0x48 %d %d" command data  
    let inline receive command = 0 //I2CLockCall wrap_I2c_ReceiveData command

let inline konst c _ = c

//TODO: do not use int, rewrite with ^T
let inline limit l u (x:int) = Math.Min(u, Math.Max (l, x))  

[<Measure>]
type ms
let inline milliseconds x = LanguagePrimitives.Int32WithMeasure<ms> x

[<Measure>]
type percent
let inline percent min max v = 
    let v' = limit min max v
    LanguagePrimitives.Int32WithMeasure<percent>(100 * (v - min)/(max - min))