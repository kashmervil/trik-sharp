module Extern 

open System
open System.Runtime.InteropServices

let I2CLockObj = new Object()

[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_init(string, int, int)
[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern void private wrap_I2c_SendData(int, int, int) 
[<DllImport("libconWrap.so.1.0.0", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)>]
    extern int private wrap_I2c_ReceiveData(int)
let I2CLockCall f args = lock I2CLockObj <| fun () -> f args

let init string deviceId forced = I2CLockCall wrap_I2c_init (string, deviceId, forced)
let send command data len = I2CLockCall wrap_I2c_SendData (command, data, len)
let receive = I2CLockCall wrap_I2c_ReceiveData 

let isLinux = (Environment.OSVersion.VersionString = "Microsoft Windows NT 6.2.9200.0")
let linux f = if isLinux then f() else ()