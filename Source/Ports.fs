namespace Trik
    ///<summary>Type representing Encoders ports</summary>
    type Encoder  = B1| B2 | B3 | B4 with 
        member self.I2cNumber = 
            match self with 
                | B1 -> 0x30
                | B2 -> 0x31 
                | B4 -> 0x32 
                | B3 -> 0x33
            static member Values = [| B1; B2; B3; B4 |]

    ///<summary>Type representing PowerMotor ports</summary>
    type Motor = M1 | M2 | M3 | M4 with
        member self.I2CNumber = 
            match self with 
            | M1 -> 0x14 
            | M2 -> 0x15
            | M3 -> 0x16
            | M4 -> 0x17
        static member Values = [| M1; M2; M3; M4 |]

    type Servo = E1 | E2 | E3 | C1 | C2 | C3 with
        member self.Path = 
            match self with 
            | E1 -> "/sys/class/pwm/ehrpwm.1:1" 
            | E2 -> "/sys/class/pwm/ehrpwm.1:0" 
            | E3 -> "/sys/class/pwm/ehrpwm.0:1"             
            | C1 -> "/sys/class/pwm/ecap.0" 
            | C2 -> "/sys/class/pwm/ecap.1" 
            | C3 -> "/sys/class/pwm/ecap.2" 
        static member Values = [| E1; E2; E3 |]

    ///<summary>Type representing AnalogSensors ports</summary>
    type Sensor = A1 | A2 | A3 | A4 | A5 | A6 with
        member self.I2CNumber = 
            match self with 
            | A1 -> 0x25
            | A2 -> 0x24
            | A3 -> 0x23
            | A4 -> 0x22
            | A5 -> 0x21
            | A6 -> 0x20
        static member Values = [| A1; A2; A3; A4; A5; A6 |]


    type VideoSource = USB = 0 | VP1 = 1 | VP2 = 2
