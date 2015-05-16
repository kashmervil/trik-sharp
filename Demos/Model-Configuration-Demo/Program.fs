//this example doesn't illustrate another one library's use case
//but the way  you can extend model class
open Trik
open Trik.Collections
open System.Collections.Generic



let model = new Model()

//////////////////////////////////////////////////
///             SERVO CONFIGURATION            ///
//////////////////////////////////////////////////

// 1) You can change servo's characteristics by specifying ServoKind, 
//    which consists of several constants (e.g min, max, zero ...) 
//    describing individual servo drive.
model.ServosConfig.[E1] <- Defaults.Servo4

// 2) In fact you can replace the default (E1..E3, C1..C3) servo ports. 
//    with absolutely new ones  
type NewServoKey = EE1 | EE2 | EE3 with
    interface IServoKey with
        member self.Path = "Just an example"

model.ServosConfig <- new Dictionary<_,_>()
model.ServosConfig.Add(EE1, Defaults.Servo1)
model.ServosConfig.Add(EE2, Defaults.Servo1)

// 3) You can even combine new ports with predefined ones.
model.ServosConfig.Add(E2, Defaults.Servo3)  // port from library

// 4) Accessing servos in seamless
model.Servos.[E1].SetPower(0)
model.Servos.[EE2].SetPower(0)

