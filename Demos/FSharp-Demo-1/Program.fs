open System
open Trik
open Trik.Collections

let log s = printfn s

[<EntryPoint>]
let main _ = 
    log "Started"
    use model = new Model(ServoConfig = [| (E1, ("/sys/class/pwm/ehrpwm.1:1", Defaults.Servo5))
                                           (E2, ("/sys/class/pwm/ehrpwm.1:0", Defaults.Servo6)) |])
    log "Loaded"
    let lt = Linetracer.Linetracer(model)
    lt.Run()
    0
