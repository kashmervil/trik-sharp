namespace Trik
open System
open System.IO
open System.Net

type FtpClient(host: string, name: string, pass: string) = 
    let bufferSize = 1024
    member self.Upload(path : string) = 
        try
            let localStream = new IO.FileStream(path, FileMode.Open
                                                  , FileAccess.Read
                                                   , FileShare.Read)

            let namePosition = path.LastIndexOfAny( [| '\\'; '/' |])  + 1
            let fileName = path.[namePosition ..]
            let ftpRequest = WebRequest.Create(String.Format(@"ftp://{0}/{1}", host, fileName))
            ftpRequest.Credentials <- NetworkCredential(name, pass)
            ftpRequest.Method <- WebRequestMethods.Ftp.UploadFile
            let ftpStream = ftpRequest.GetRequestStream()
            let buffer = Array.zeroCreate bufferSize
            let rec loop() = 
                async {
                    let! cnt = localStream.AsyncRead(buffer, 0, bufferSize)
                    if cnt > 0 then
                        do! ftpStream.AsyncWrite(buffer, 0, cnt)
                        return! loop()
                }
            loop() |> Async.RunSynchronously
            localStream.Close()
            ftpStream.Flush()
            ftpStream.Close()

        with e -> printfn "Upload failed: %s"<| e.ToString()

    member 
