# EloquentObjects
EloquentObjects is a .NET fast and lightweight IPC framework that allows clients to work with hosted objects remotelly (call methods, get or set properties, subscribe to events, etc.). Can be used as an object-oriented replacement to traditional RPC (Remote Procedure Call) mechanisms.

## Target Frameworks
- .NET Framework 4.5+
- .NET Standard 2.0+

## Installation

Packages are available on NuGet: [`EloquentObjects`](https://www.nuget.org/packages/EloquentObjects). You can use the following command in the Package Manager Console:
`Install-Package EloquentObjects`

## Concepts
EloquentObjects can host objects that implement attributed interfaces (like in WCF) using TCP (for RPC) or named pipes (for IPC) bindings.
Multiple clients can connect to the same hosted object remotelly. Each hosted object has an object ID that is used by clients to distinguish between hosted objects.

Following features are supported:
* Call methods and get responses.
* Receive and rethrow exception if it occured in hosted object method
* Call one-way methods (responses and exceptions are not sent to client for such methods).
* Subscribe to hosted objects events (EventHandler, EventHandler<T> and Action events are supported).

Note that EloquentObjects behave differently from traditional Remote Procedure Call (RPC) implementations, for example:
* In RPC, the client makes a request and waits for the response.
* In RPC, the server doesn't push anything to the client unless it's in response to a request.
* Often, the design of RPC is such that different clients are served by independent service instances.

Object hosting can be started with just few lines of code:

```csharp
//Create a server that will run on 127.0.0.1:50000 in RPC mode (i.e. using TCP binding)
using (var remoteObjectServer = new EloquentServer("tcp://127.0.0.1:50000"))
{
	//Start hosting for the given object with given Object ID that will be used by client to access this object.
	remoteObjectServer.Add<IYourContractHere>("<Your Object ID here>", <You object here>);
	
	//Keep the server running
	while(true) { }
}
```

When object is hosted you can connect to remote object:

```csharp
//Create a client that will listen receive object events on "tcp://127.0.0.1:50001"
//Client will keep a communication session with the server.
//One client can connect to multiple objects (distinguished by object ID which can be any string)
using (var client = new EloquentClient("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001"))
{
	//Connect and use the object.
	//The 'yourObject' will have IYourContractHere type below:
	var yourObject = client.Connect<IYourContractHere>("<Your Object ID here>");
}
```    


## Quick start example
1. Create Server, Client and Contract assemblies. Add dependency from Contract assembly both to Server and to Client.
2. Define an attributed contract in a Contract assembly that is available both for server and client:

```csharp 
public interface IEloquentCalculator
{
	string Name { get; set; }

	int Add(int a, int b);
	
	void Sqrt(int a);

	event EventHandler<OperationResult> ResultReady;
}
    
[DataContract]
public sealed class OperationResult
{
	public OperationResult(double value)
	{
		Value = value;
	}
	
	[DataMember]
	public double Value { get; private set; }
}
```

Note that complex data DTOs (e.g. OperationResult in example above) can be used as properties, method parameters, method return values and event parameters.
    
3. Implement a contract in Server assembly.

```csharp
internal sealed class EloquentCalculator: IEloquentCalculator
{
	#region Implementation of ICalculatorService
	...
	#endregion
}
```

4. Create a server and start an object hosting in a Server assembly:
    
```csharp
//Create an object that will be hosted
var calculator = new EloquentCalculator();

//Create a server that will run on 127.0.0.1:50000 using TCP binding
using (var remoteObjectServer = new EloquentServer("tcp://127.0.0.1:50000"))
{
	//Start hosting for the calculator with Object ID = Calculator1
	remoteObjectServer.Add<IEloquentCalculator>("Calculator1", calculator);
	
	//Keep the server running
	Console.ReadLine();
}
```

5. Connect to a hosted object from Client assembly:

```csharp
//Create a client that will listen to object events on "tcp://127.0.0.1:50001"
using (var client = new EloquentClient("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001"))
{
	//Use the same Object ID - Calculator1
	var calculator = client.Connect<IEloquentCalculator>("Calculator1");
		
	//Work with calculator remotelly
	var res1 = calculator.Add(1, 1);
	calculator.ResultReady += (s, r) => {...}
}
```    

## Bindings
EloquentObjects support twos communication mechanisms:
* TCP binding (RPC)
* Named pipes binding (IPC)

Binding is selected by URI scheme in address as shown in examples below:

```csharp
//Start server with TCP binding:
using (var tcpServer = new EloquentServer("tcp://127.0.0.1:50000")) 
{
	...
}

//Start server with Named pipes binding:
using (var pipesServer = new EloquentServer("pipe://127.0.0.1:50000")) 
{
	...
}

//Create client with TCP binding:
using (var client = new EloquentClient("tcp://127.0.0.1:50000", "tcp://127.0.0.1:50001"))
{
	...
}    

//Create client with Named pipes binding:
using (var client = new EloquentClient("pipe://127.0.0.1:50000", "pipe://127.0.0.1:50001"))
{
	...
}    
```

## Patterns
[TBD]
Use following patterns with EloquentObject to get best results:
1. Events handling
2. Accessing child objects
3. Hosting model layer
4. Interface inheritance

## Events handling
Eloquent object can have events of following types:
* EventHandler
* EventHandler<T>
* Action (with any number of arguments)

```csharp
public interface IContract
{
	event EventHandler RegularEvent;

	event EventHandler<CustomEventArgs> RegularEventWithArgs;

	event Action NoParameterEvent;

	event Action<int> EventWithIntParameter;
}
```

When event handler is called for EventHandler and EventHandler<T> event types on client side then the sender parameter will contain a proxy object. So Event Handling pattern can be used to operate with the sender.

```csharp
//The following method will handle following subscribtions:
//remoteObject1.RegularEvent += OnRegularEvent;
//remoteObject2.RegularEvent += OnRegularEvent;

void OnRegularEvent(object sender, EventArgs args)
{
    //Here object will be remoteObject1 when event occured in remoteObject1 and will be remoteObject2 when event occured in remoteObject2.
    var object = (IContract)sender;
}
```

# Modification history
[!] - Breaking API change<br/>
[+] - New feature<br/>
[B] - Bug fix<br/>

## 2.0.0
[!] Removed attributes from contracts. Standard C# interfaces can be used now.<br/>
[!] Changed client API. No need to create a disposable connection anymore.<br/>
[+] Implemented ability to transfer objects by references (DTO objects are still supported).<br/>

## 1.0.4
[+] Added test for default parameters in interface<br/>
[+] Cleaned the Calculator example to demonstrate the HostingModel layer.<br/>

## 1.0.3
[+] Added robustness integration tests<br/>
[B] Fixed exception on a client when server is lost<br/>
[B] Fixed exception on a client when server stopped hosting object<br/>
[B] Fixed exception on a client when server does not host any objects for requested ID<br/>

## 1.0.2
[!] Renamed EloquentInterfaceAttribute to EloquentContractAttribute<br/>
[+] Added named pipes transport protocol support<br/>
[+] Added integration tests<br/>
[+] Added support for EventHandler and EventHandler<T> event types<br/>

## 1.0.1
[!] Channged scheme in URI address to contain "xxx://" prefix (tcp is used for TCP transport protocol, pipe is used for named pipes)<br/>

## 1.0.0
[+] Initial release.<br/>

# TODO
## Features TODO
1. Security
2. Polling mode
3. Client event that connection is lost. Restore connection.

## Improvements TODO
1. Timeouts?
2. Benchmark: gRPC
3. Benchmark: .NET Remoting
4. Named pipes between different PCs
5. Support out parameters