module Trik.Tasking

type ExecutionStatus<'T> = Normal of 'T | Break | ExitTask of 'T

[<NoComparison; NoEquality>]
type Task = Task of (unit -> ExecutionStatus<unit>) with
    
    static member Start (t:Task) = t.Start()
    static member StartAndWait(t:Task) = t.StartAndWait()
     
    member self.Start() = match self with Task x -> async {x() |> ignore} |> Async.Start
    member self.StartAndWait() = match self with Task x -> async {x() |> ignore} |> Async.RunSynchronously
     

[<AutoOpen>]
module internal builder = 
    let bind x f = match x with  
                   | Normal r -> f r 
                   | Break as a -> a
                   | ExitTask (t: 'a) as b -> b
    
type TaskBuilder() = 
    member self.Bind(x, f) = bind x f
   
    member self.Combine(c1, c2) = self.Bind(c1, c2)
                                  
    member self.Delay(x) = x
    
    member self.Run(x) = Task x
    
    member self.Zero() = Normal ()
    
    member self.Yield(x) = Normal x

    member self.Return(x) = Normal x

    member self.ReturnFrom(x) = x

    member self.TryWith(body, handler) =
        try self.ReturnFrom(body())
        with e -> handler e

    member self.TryFinally(body, func) =
        try self.ReturnFrom(body())
        finally func() 

    member self.Using(disposable: 'T when 'T:> System.IDisposable, body) =
        let body' = fun () -> body disposable
        self.TryFinally(body', fun () -> 
            match disposable with 
                | null -> () 
                | disp -> disp.Dispose())

    member self.While(guard, cexpr) = 
        let counter = ref 0
        let rec loop result = match result with
                              | Normal () when guard() -> incr counter;
                                                          loop <| cexpr()
                              | Break -> Normal ()
                              | Normal _ as d -> Normal ()
                              | ExitTask _ as x -> x
        loop <| Normal()

    member self.For(collection: seq<_>, func) = 
        self.Using(collection.GetEnumerator(), fun en -> 
            self.While((fun () -> en.MoveNext()), self.Delay(fun () -> func en.Current)))
    

let task = new TaskBuilder()

let BREAK = Break
let EXIT = ExitTask ()
