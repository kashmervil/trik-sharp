Trik-Sharp
===
## 
Library for Robotics programming in F#
---
**Trik-Sharp** is designed with strong attempt to cope with Rx and   [TRIK-controller](http://www.trikset.com). 
It provides general methods for working with sensors (_IR, Gyroscope, Accelerometer, Light and Noise sensors, Encoders etc._) along with output devices (_Servomotors and general power motors, Led stripe and On-board light bulb_). <Enter> 

Trik-Sharp can also be freely used without Reactive Extensions due to providing _not event-based_ sensors communication. 

Main development language is F# but trik-sharp isn't just F# faced library. All components as well can be effectively used in C# in a natural not disruptive way



##Build tips

You can use library and make robots either with MacOS, Linux or Windows 

 * Use _Xamarin Studio_/_Monodevelop_ or _Visual Studio_
 * F# 3.0 / F# 3.1
 * .NET/Mono 4.0 framework (is set already in build options)
 * Enable NuGet package restore 


##Deploy & Run

###Copying files

For file transferring with Wi-Fi connected [TRIK-controller](http://www.trikset.com)  you may use  

 * _SCP_ console tool
 *  _WinSCP_ if you use Windows
 
###Deploying for the first time
  
 

 * Set up _**ssh**_ tunnel with controller board (linux _**ssh**_ tool or _**Putty**_ can be used)
 * Copy to a robot all files in a folder you got from build 
 * Make sure folder contains _**Trik.Core.dll**_ and _**libconWrap.so**_.  If it does not then copy these assembly too
 * Copy to a robot _**Fsharp.Core.dll**_ which can be find GAC
 
 
###Running

After you end up with file moving. You can run your application (e.g test.exe) with Mono environment 
```
root@trikboard:~/home/root/test/# mono test.exe
```

###Future development

For future deployment you only need to move new versions of your program. All external components will remain the same.

You can also make .sh script file with your program to make it accessible from _On-board_ file explorer. So you no longer need connected laptop to run your app

```
root@trikboard:~/home/root/test/# echo '#!/bin/sh
> mono test.exe' > test.sh 
root@trikboard:~/home/root/test/# chmod +x test.sh
```

