open Trik.Junior


printfn "Press any key to start"
robot.Uploader.Host <- "8.8.8.8" // Enter your configuration of ftp profile
robot.Uploader.Login <- "login"
robot.Uploader.Pass <- "password"

while System.Console.ReadLine() <> "q" do
    robot.ShotAndUpload()               // After calling robot send photo to specified server
    printfn "Press any key to continue or q to exit"