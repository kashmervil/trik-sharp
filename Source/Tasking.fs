module Trik.Tasking

type ExecutionStatus<'T> = Normal of 'T | Break | ExitTask of 'T

[<NoComparison; NoEquality>]
type Task = Task of (unit -> ExecutionStatus<unit>) with
    
    static member Start (t:Task) = t.Start()
    static member StartAndWait(t:Task) = t.StartAndWait()
     
    member self.Start() = match self with Task x -> async {x() |> ignore} |> Async.Start
    member self.StartAndWait() = match self with Task x -> async {x() |> ignore} |> Async.RunSynchronously

    
type TaskBuilder() = 
    member self.Bind(x, f) = match x with  
                             | Normal r -> f r 
                             | Break as a -> a
                             | ExitTask (t: 'a) as b -> b
  
    member self.Combine(c1, c2) = self.Bind(c1, c2)
                                  
    member self.For(collection: seq<_>, func) = 
        let en = collection.GetEnumerator()
        self.While((fun () -> en.MoveNext()), self.Delay(fun () -> let value = en.Current in func value))

    member self.Delay(x) = x
    member self.Run(x) = Task x

    member self.Return(x) = Normal x

    member self.While(guard, cexpr) = 
        let counter = ref 0
        let rec loop result = match result with
                              | Normal () when guard() -> incr counter;
                                                          loop <| cexpr()
                              | Break -> Normal ()
                              | Normal _ as d -> Normal ()
                              | ExitTask _ as x -> x

        loop <| Normal()
    member self.Zero() = Normal ()
    member self.ReturnFrom(x) = x
    member self.Yield(x) = Normal x
    
let task = new TaskBuilder()

let BREAK = Break
let EXIT = ExitTask ()