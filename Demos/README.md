Trik-Sharp Demos
===

This folder consists of different programs from basic examples to complete robots that have been used in a range of events and conferences. 

These Demos are supposed to help you understand the ways of interacting with peripherals and using functionality exposed by this library. It can also show some common use cases for you to better understand **trik-sharp** from an architectural point of view, which can be even more profitable.

Examples are written in different languages to make you more comfortable with it in case you don't know C# or F# (technically it runs well on any other .NET language, and I’d be glad if someone shared such demos with me). Some of the examples are implemented in both of C# and F#, so you can make line-by-line comparison to notice differences in languages and ways that library handles them.

Here is the list of available demos

+ [CSharp-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/CSharp-Demo) and [FSharp-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/FSharp-Demo) -- programs that use Rx and distance sensor for avoiding obstacles.

+ [Presentation-Demo-noRx](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Presentation-Demo-noRx) -- program for robot to move parallel to the wall. Implements a basic logic and shows users that **trik-sharp** implements coolest features of Rx without lack of performance on a robot, as it would be if we used Rx (you can check out code with Rx here)

+ [Empty-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Empty-Demo) -- simple programs (C#, F#) that make on-board led blink. 

+ [Segway-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Segway-Demo) -- a working model that solves reverse pendulum problem. This example has special technical requirement and can be shown as a proof of the concept for .NET robotics programming.

+ [Stripe-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Stripe-Demo) -- model that changes color depending on it position (according to the gyroscope).

+ [Manipulator-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Manipulator-Demo) -- an example of robot controlled via android app called [trik-gamepad](). The robot uses two servomotors to control its “hand” using the signals sent from mobile app.

+ [Hunter-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Hunter-Demo) --  one of the most interesting demo in this series. It tracks the object movement via camera using `object-sensor`, sensor that determines objects by their color. It stares to and follows the object like a hunter (what's where the name came from). 

+ [MxN-Sensor-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/MxN-Sensor-Demo) -- this program allows you to play around with `MxN-sensor` (a.k.a `color-sensor` in TRIK Studio), increasing a number of dominant-color searching zones in two dimensions by pressing `Left` and `Right` buttons.

+ [LineSensor-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/LineSensor-Demo) -- program that print out to console HSV and correspondent tolerance values from `line-sensor`

+ [Model-Configuration-Demo](https://github.com/kashmervil/trik-sharp/tree/master/Demos/Model-Configuration-Demo) -- program to understand how one can configure his servomotors (same applies to almost all of peripherals, you can check the implementation in the library)


Demos with addition of Intellisense and [ReadMe](https://github.com/kashmervil/trik-sharp/blob/master/README.md) aim to simplify your learning curve and take you closer to development. 

Happy coding!