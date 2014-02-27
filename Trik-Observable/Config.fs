module Trik.Config

open System
open System.Xml
open FSharp.Data
open System.Xml.Linq
open System.Diagnostics
open Helpers

type Config = XmlProvider<"config.xml">

let private Syscall_system cmd  = 
    let args = sprintf "-c '%s'" cmd
    let proc = Process.Start("/bin/sh", args)
    proc.WaitForExit()
    if proc.ExitCode  <> 0 then
        printf "Init script failed at '%s'" cmd
    else printfn " Done"

let runInitScript (config:Config.DomainTypes.Config) = 
    config.InitScript.Split([| '\n' |])
    |> Array.iter  (fun s -> if not <| String.IsNullOrWhiteSpace s then Syscall_system s)
    
let load () = Config.Load "config.xml"
