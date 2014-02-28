module Trik.Config

open System
open System.Xml
open FSharp.Data
open System.Xml.Linq
open System.Diagnostics
open Helpers

type internal Provider = XmlProvider<"config.xml">
type Schema = Provider.DomainTypes

let private Syscall_system cmd  = 
    let args = sprintf "-c '%s'" cmd
    trikSpecific <| fun () ->
        let proc = Process.Start("/bin/sh", args)
        proc.WaitForExit()
        if proc.ExitCode  <> 0 then
            printf "Init script failed '%s'" cmd
        else printfn " Done"

let internal Create (path:string) =
    let config = Provider.Load path
    config.InitScript.Split([| '\n' |]) 
    |> Array.iter  (fun s -> if not <| String.IsNullOrWhiteSpace s then Syscall_system s)
    config