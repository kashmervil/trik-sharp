module Sensor3d

open System
open System.IO
open System.Reactive.Linq
open System.Diagnostics
let event_size = 16
let ev_abs = uint16 3 
let abs_x = uint16 0 
let abs_y = uint16 1 
let abs_z = uint16 2 

(*
input_event: 16
event:-1095001840
event.type:-1095001832
EV_ABS:3
event.code:-1095001830
event.value:-1095001828
ABS_X:0
ABS_Y:1
ABS_Z:2
*)

[<AllowNullLiteralAttribute>]
type Sensor3d (min, max, deviceFile, rate) = 
   
    let stream = File.Open(deviceFile, FileMode.Open) 
    let mutable last = Array.create 3 0
    let bytes = Array.create event_size (byte 0)    

    let readFile() =  
        let readCnt = stream.Read(bytes, 0, bytes.Length)
        if readCnt <> event_size then
            failwith "event reading error\n"
        else
            let evType = BitConverter.ToUInt16(bytes, 8)
            let evCode = BitConverter.ToUInt16(bytes, 10)
            let evValue = BitConverter.ToInt32(bytes, 12)
            //printfn "evType: %A" evType
            match evType with
            | x when x = ev_abs -> 
                match evCode with
                | 0us -> (last.[0] <- evValue)
                | 1us -> (last.[1] <- evValue)
                | 2us -> (last.[2] <- evValue)
                | _ -> ()
            | _ -> ()
            (last.[0], last.[1], last.[2])

    let mutable obs:IObservable<int*int*int> = null            
    member this.Start() = 
        (readFile(), readFile(), readFile(), readFile(), readFile(), readFile(), readFile(), readFile()) |> ignore
        let sw = new Stopwatch()
        sw.Start()
        obs <- Observable.Generate(0, Func<_,bool>(fun _ -> true), Func<int,int>(fun x -> x)
            , Func<int,_>(fun _ -> readFile()) )//, Func<_,TimeSpan>(fun _ -> System.TimeSpan.FromMilliseconds(rate)))
        printfn "Observable.Generate: %A" sw.Elapsed
    member this.Obs = obs

(*

class TRIKCONTROL_EXPORT Sensor3d : public QObject
{
	Q_OBJECT

public:
	/// Constructor.
	/// @param min - minimal actual (physical) value returned by sensor. Used to normalize returned values.
	/// @param max - maximal actual (physical) value returned by sensor. Used to normalize returned values.
	/// @param deviceFile - device file for this sensor.
	Sensor3d(int min, int max, QString const &deviceFile);

public slots:
	/// Returns current raw reading of a sensor in a form of vector with 3 coordinates.
	QVector<int> read();

private slots:
	/// Updates current reading when new value is ready.
	void readFile();

private:
	QSharedPointer<QSocketNotifier> mSocketNotifier;
	QVector<int> mReading;
	int mDeviceFileDescriptor;
	int mMax;
	int mMin;
};

}
Sensor3d::Sensor3d(int min, int max, const QString &controlFile)
	: mDeviceFileDescriptor(0)
	, mMax(max)
	, mMin(min)
{
	mReading << 0 << 0 << 0;

	mDeviceFileDescriptor = open(controlFile.toStdString().c_str(), O_SYNC, O_RDONLY);
	if (mDeviceFileDescriptor == -1) {
		qDebug() << "Cannot open input file";
		return;
	}

	mSocketNotifier = QSharedPointer<QSocketNotifier>(
			new QSocketNotifier(mDeviceFileDescriptor, QSocketNotifier::Read, this)
			);

	connect(mSocketNotifier.data(), SIGNAL(activated(int)), this, SLOT(readFile()));
	mSocketNotifier->setEnabled(true);
}

void Sensor3d::readFile()
{
	struct input_event event;

	if (::read(mDeviceFileDescriptor, reinterpret_cast<char *>(&event), sizeof(event)) != sizeof(event)) {
		qDebug() << "incomplete data read";
	} else {
		switch (event.type)
		{
		case EV_ABS:
			switch (event.code)
			{
			case ABS_X:
				mReading[0] = event.value;
				break;
			case ABS_Y:
				mReading[1] = event.value;
				break;
			case ABS_Z:
				mReading[2] = event.value;
				break;
			}
			break;
		case EV_SYN:
			return;
		}
	}
}

QVector<int> Sensor3d::read()
{
	return mReading;
}

*)
