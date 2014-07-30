open System
open Trik
open Trik.Junior
open Trik.Junior.Parallel

let flicker = task { while true do
                        for x in [1..3] do
                        printfn "current %d" x
                        Threading.Thread.Sleep(1000)//robot.Sleep(500)
                    }
let disp = flicker.Start()

let drive = task { while true do
                        let i = ref 10
                        while !i > 0 do
                            printfn "iteration num. %d" !i
                            decr i
                        if 8 < System.Random().Next(100) then
                            do! EXIT
                        printfn "from outer loop"
                        System.Threading.Thread.Sleep(1000)//robot.Sleep(300)
                    }


flicker.Execute()
drive.StartAndWait()
System.Console.Read() |> ignore



