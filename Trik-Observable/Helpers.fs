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
let I2CLockCall f args : 'T = 
        if isLinux then lock I2CLockObj <| fun () -> f args
        else Unchecked.defaultof<'T>


module I2C = 
    let inline init string deviceId forced = I2CLockCall wrap_I2c_init (string, deviceId, forced)
    let inline send command data len = I2CLockCall wrap_I2c_SendData (command, data, len)
    let inline receive command = I2CLockCall wrap_I2c_ReceiveData command

let inline konst c _ = c

//TODO: do not use int, rewrite with ^T
let inline limit l u (x:int) = Math.Min(u, Math.Max (l, x))  
