open Trik
open System.Threading

let led = new Led()
for i in [1..3] do
    for color in [LedColor.Green; LedColor.Orange; LedColor.Red] do
        led.SetColor color
        Thread.Sleep 300

led.SetColor LedColor.Off