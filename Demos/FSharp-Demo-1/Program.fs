open System
open Trik


let log s = printfn s

[<EntryPoint>]
let main _ = 
    log "Started"
    Helpers.I2C.Init "/dev/i2c-2" 0x48 1
    use model = new Model(ServoConfig = [| 
                              ("JE1", "/sys/class/pwm/ehrpwm.1:1", 
                                { stop = 0; zero = 1310000; min = 1200000; max = 1420000; period = 20000000 } )
                              ("JE2", "/sys/class/pwm/ehrpwm.1:0", 
                                { stop = 0; zero = 1550000; min =  800000; max = 2250000; period = 20000000 } )
                             |])
    log "Loaded"
    let lt = Linetracer.Linetracer(model)
    lt.Run()
    0
