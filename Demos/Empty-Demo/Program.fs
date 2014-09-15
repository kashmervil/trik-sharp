open Trik
open System.Threading

let led = new Led("/sys/class/leds/")

for color in let cs = [LedColor.Green; LedColor.Orange; LedColor.Red] in cs @ cs @ cs do
    led.SetColor color
    Thread.Sleep 300

led.SetColor LedColor.Off