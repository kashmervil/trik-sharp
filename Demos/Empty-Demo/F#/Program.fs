open System
open System.Threading
open Trik
open Trik.Devices


let led = new Led()
for i in [1..5] do
    for color in [LedColor.Green; LedColor.Orange; LedColor.Red] do
        led.SetColor color
        Console.WriteLine(color.ToString())
        Thread.Sleep 500

led.SetColor LedColor.Off