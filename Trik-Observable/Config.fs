module Config

open System
open System.Xml
open FSharp.Data
open System.Xml.Linq
open System.Diagnostics
open Extern

type Config = XmlProvider<"config.xml">

let Syscall_system cmd  = 
    let args = sprintf "-c '%s'" cmd
    printf "%s" cmd
    let proc = Process.Start("/bin/sh", args)
    proc.WaitForExit()
    if proc.ExitCode  <> 0 then
        printf "Init script failed at '%s'" cmd
    else ()
    printfn " Done"

let runInitScript (config:Config.DomainTypes.Config) = 
    config.InitScript.Split([| '\n' |])
    |> Seq.map (fun s -> s.Trim() )
    |> Seq.filter (not << String.IsNullOrEmpty)
    |> Seq.iter Syscall_system


let config = Config.Load "config.xml"
