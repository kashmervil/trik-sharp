module Trik.Tasking
open System.Threading

type ExecutionStatus<'T> = Normal of 'T | Break | ExitTask of int | Cancelled

type ReadyToStart<'T> =  unit -> CancellationToken -> ExecutionStatus<'T>


[<NoComparison; NoEquality>]
type private TaskImpl<'T> = (CancellationToken -> ExecutionStatus<'T>)

[<NoComparison; NoEquality>]
type Task<'T> = Task of (CancellationToken -> ExecutionStatus<'T>) with
    
    static member Start (t: ReadyToStart<_>) = (Task <| t()).Start()
    static member StartAndWait(t: ReadyToStart<_>) = (Task <| t()).StartAndWait()
    member self.Start() = 
        let cts = new CancellationTokenSource()
        match self with Task x -> let t = async {x(cts.Token) |> ignore} in Async.Start(t, cts.Token)
        {new System.IDisposable with
            member self.Dispose() = cts.Cancel()
        }

    member self.StartAndWait() = 
        let cts = new CancellationTokenSource()
        match self with Task x -> let t = async {x(cts.Token) |> ignore} in Async.RunSynchronously(t, cancellationToken = cts.Token)
        {new System.IDisposable with
            member self.Dispose() = cts.Cancel()
        }


let inline private wrapStatus status (token : CancellationToken) = if token.IsCancellationRequested 
                                                                   then Cancelled else status
[<AutoOpen>]// TODO
module private BuilderImpl = 
    

    let bindT (prev : TaskImpl<'a>) (f: 'a -> TaskImpl<'b>) token=
        match prev token with
        | Normal r ->  f r token
        | Break -> Break
        | ExitTask n -> ExitTask n 
        | Cancelled  -> Cancelled
        
    let delayT x = x

    let tryWithT body handler token = 
        try body () token
        with e -> handler e token

    let tryFinallyT body compensation token = 
        try body () token
        finally compensation ()

    let usingT (disposable: 'T when 'T:> System.IDisposable) body token =
        let body' = fun () -> body disposable
        tryFinallyT body' (fun () -> 
            match disposable with 
                | null -> () 
                | disp -> disp.Dispose()) token

    let whileT guard (cexpr : unit -> TaskImpl<_>) token = 
        let rec loop result = match result with
                              | Normal () when guard() -> loop <| cexpr () token
                              | Break -> Normal ()
                              | Normal _ -> Normal ()
                              | ExitTask n -> ExitTask n
                              | Cancelled -> Cancelled
        loop <| Normal()     



    
type TaskBuilder() = 
    member self.Bind(x, f) = bindT x f
   
    member self.Combine(c1, c2) = bindT c1 c2
                                  
    member self.Delay(x) = x
    
    //member self.Run(x) = Task x
    
    member self.Zero() = wrapStatus <| Normal ()
    
    member self.Yield(x) = wrapStatus <| Normal x

    member self.Return(x) = wrapStatus <| Normal x

    member self.ReturnFrom(x) = delayT x

    member self.TryWith(b, h) = tryWithT b h

    member self.TryFinally(b, c) = tryFinallyT b c

    member self.Using(d, b) = usingT d b

    member self.While(guard, cexpr) = whileT guard cexpr

    member self.For(collection: seq<_>, func) = 
        self.Using(collection.GetEnumerator(), fun en -> 
            self.While((fun () -> en.MoveNext()), self.Delay(fun () -> func en.Current)))
    

let task = new TaskBuilder()

let BREAK (token: CancellationToken) = if token.IsCancellationRequested 
                                       then Cancelled else Break

let EXIT (token: CancellationToken) = if token.IsCancellationRequested 
                                       then Cancelled else ExitTask 0
