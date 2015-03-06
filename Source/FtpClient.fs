namespace Trik.Network
open System
open System.IO
open System.Net

type FtpClient() = 
    let bufferSize = 1024
    let uploader path host name (pass: string) = 
        async {
            try
                let localStream = new IO.FileStream(path, FileMode.Open
                                                      , FileAccess.Read
                                                       , FileShare.Read)

                let namePosition = path.LastIndexOfAny( [| '\\'; '/' |])  + 1
                let fileName = path.[namePosition ..]
                let ftpRequest = FtpWebRequest.Create(String.Format(@"ftp://{0}/{1}", host, fileName))
                ftpRequest.Credentials <- NetworkCredential(name, pass)
                ftpRequest.Method <- WebRequestMethods.Ftp.UploadFile
                let ftpStream = ftpRequest.GetRequestStream()
                let buffer = Array.zeroCreate bufferSize
                let rec loop() = 
                        let cnt = localStream.Read(buffer, 0, bufferSize)
                        if cnt > 0 then
                            ftpStream.Write(buffer, 0, cnt)
                            ftpStream.Flush()
                            loop()
                  
                loop()
                localStream.Close()
                
                ftpStream.Close()

            with e -> printfn "Upload failed: %s" <| e.ToString()
        }
    member val Host = "" with get, set
    member val Login = "" with get, set
    member val Pass  = "" with get, set

    member self.AsyncUpload(path: string) = uploader path self.Host self.Login self.Pass 
    member self.Upload(path : string) = self.AsyncUpload path |> Async.RunSynchronously
