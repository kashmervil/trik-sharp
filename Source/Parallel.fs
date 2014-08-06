module Trik.Junior.Parallel
open System
open System.Threading
open Trik.Junior
open Trik

type ExecutionStatus<'T> = Normal of 'T | Break | ExitTask of int | Cancelled

type RunningTask(d: IDisposable) = 
    member self.Stop() = d.Dispose()
and 
    [<NoComparison; NoEquality>]
    Task<'T>(f: CancellationToken -> ExecutionStatus<'T>) =

        let helper timeout = 
            let timeout = defaultArg timeout -1  
            let cts = new CancellationTokenSource()
            cts.CancelAfter(timeout) 
            async {f cts.Token |> ignore}, cts

        member self.Start(?milliseconds) =
            let a, cts = helper milliseconds
            Async.Start(a, cts.Token)
            let d = { new System.IDisposable with
                            member self.Dispose() = cts.Cancel()}
            Robot.RegisterResource d
            new RunningTask(d)
        
        member self.Execute() = self.Start() |> ignore 

        member self.Value = f
        member self.StartAndWait(?milliseconds) =
            let a, cts = helper milliseconds
            let d = { new System.IDisposable with
                            member self.Dispose() = cts.Cancel()}
            Robot.RegisterResource d
            Async.RunSynchronously a

        static member (<+>) ((a: Task<_>), (b: Task<_>)) = Group (a::b::[])
        static member (<+>) ((a: Task<_>), (Group b: TaskGroup<_>)) = Group (a::b)
        static member (<+>) ((Group b: TaskGroup<_>), (a: Task<_>)) = Group (a::b)

    and 
        [<NoComparison; NoEquality>]
        TaskGroup<'T> = Group of list<Task<'T>> with
            member self.StartAndWait(?milliseconds) = 
                let timeout = defaultArg milliseconds -1  
                let cts = new CancellationTokenSource()
                cts.CancelAfter(timeout) 
                let (Group allTasks) = self
                let d = { new System.IDisposable with
                            member self.Dispose() = cts.Cancel()}
                Robot.RegisterResource d

                allTasks |> List.map (fun f -> async {f.Value cts.Token |> ignore})
                |> Async.Parallel |> Async.RunSynchronously |> ignore

            static member (<+>) ((Group a: TaskGroup<_>), (Group b: TaskGroup<_>)) = Group (a @ b)

                

let inline private wrapStatus status = Task (fun token -> if token.IsCancellationRequested 
                                                          then Cancelled else status)
[<AutoOpen>]
module private BuilderImpl = 
    let Task x = new Task<_>(x)

    let bindT (prev : Task<'a>) (f: 'a -> Task<'b>) =
        Task <| fun token -> 
        match prev.Value token with
        | Normal r ->  (f r).Value token
        | Break -> Break
        | ExitTask n -> ExitTask n 
        | Cancelled  -> Cancelled
        
    let delayT x = x

    let tryWithT (body: unit -> Task<_>) (handler:  _ -> Task<_>) = 
        Task <| fun token -> 
        try body().Value token
        with e -> handler(e).Value token

    let tryFinallyT (body: unit -> Task<_>) compensation = 
        Task <| fun token ->
        if token.IsCancellationRequested then Cancelled
        else 
            try body().Value token
            finally compensation ()

    let usingT (disposable: 'T when 'T:> System.IDisposable) body =
        let body' = fun () -> body disposable
        tryFinallyT body' (fun () -> 
            match disposable with 
                | null -> () 
                | disp -> disp.Dispose())

    let whileT guard (cexpr : unit -> Task<_>) = 
        Task <| fun token -> 
        let rec loop result = 
            if token.IsCancellationRequested then Cancelled 
            else match result with
                 | Normal () when guard() -> loop <| cexpr().Value token
                 | Break -> Normal ()
                 | Normal () -> Normal ()
                 | ExitTask n -> ExitTask n
                 | Cancelled -> Cancelled
        loop <| Normal()     



    
type TaskBuilder() = 
    member self.Bind(x, f) = bindT x f
   
    member self.Combine(c1, c2) = bindT c1 c2
                                  
    member self.Delay(x) = x
    
    member self.Run(x) = x()
    
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

let BREAK = new Task<unit> (fun token -> if token.IsCancellationRequested then Cancelled else Break)

let EXIT = new Task<unit>(fun token ->  if token.IsCancellationRequested 
                                        then Cancelled else ExitTask 0)
