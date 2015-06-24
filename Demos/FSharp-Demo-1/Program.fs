open System
open Trik


let log s = printfn s

[<EntryPoint>]
let main _ = 
    log "Started"
    use model = new Model(ServosConfig = dict [| upcast E1, Defaults.Servo5
                                                 upcast E2, Defaults.Servo6 |])
    log "Loaded"
    let lt = Linetracer.Linetracer(model)
    lt.Run()
    0
