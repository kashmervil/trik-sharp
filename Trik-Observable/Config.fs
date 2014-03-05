module Trik.Config

open System
open System.Xml
open FSharp.Data
open System.Xml.Linq
open Helpers

type internal Provider = XmlProvider<"config.xml">
type Schema = Provider.DomainTypes
        
let internal Create (path:string) =
    let config = Provider.Load path
    //config.InitScript.Split([| '\n' |]) 
    //|> Array.iter  (fun s -> if not <| String.IsNullOrWhiteSpace s then Syscall_shell s)
    config