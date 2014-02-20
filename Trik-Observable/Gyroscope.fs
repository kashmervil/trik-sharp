module Gyroscope

open Sensor3d
open Config
open Extern

let path = 
    if isLinux then config.Sensors.Gyroscope.DeviceFile 
    else @"log.txt"

type Gyroscope(rate) =
    let sensor = new Sensor3d(config.Sensors.Gyroscope.Min
                        , config.Sensors.Gyroscope.Max
                        , path, rate)
    do sensor.Start()
    member this.Obs = sensor.Obs

