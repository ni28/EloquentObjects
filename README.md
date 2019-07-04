# EloquentObjects
EloquentObjects is a .NET object-oriented Inter-Process Communication (IPC) and Remote-Procedure Call (RPC) framework that allows clients to work with hosted objects remotelly (call methods, get or set properties, subscribe to events, etc.).

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
* Often, the design of RPC is such that different clients are independent of each other.

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
		//Connect to a specific object.
		//Connection object will keep all resources needed to communicate with specific remote object until it is disposed.
		using (var connection = client.Connect<IYourContractHere>("<Your Object ID here>"))
        {
			//Use the object
			var yourObject = connection.Object;
			...
		}
	}
```	

See full tutorial here: [TBD]

## Quick start example
1. Create Server, Client and Contract assemblies. Add dependency from Contract assembly both to Server and to Client.
2. Define an attributed contract in a Contract assembly that is available both for server and client:

```csharp 
    [EloquentInterface]
    public interface IEloquentCalculator
    {
        [EloquentProperty]
        string Name { get; set; }

        [EloquentMethod]
        int Add(int a, int b);
        
        [EloquentMethod]
        void Sqrt(int a);

        [EloquentEvent]
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
		using (var calculatorConnection = client.Connect<IEloquentCalculator>("Calculator1"))
        {
			var calculator = calculatorConnection.Object;
			
			//Work with calculator remotelly
			var res1 = calculator.Add(1, 1);
			calculator.ResultReady += (s, r) => {...}
		}
	}
```	
## Bindings
EloquentObjects support two communication mechanisms:
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


## Events handling

Eloquent object can have events of following types:
* EventHandler
* EventHandler<T>
* Action (with any number of arguments)

```csharp
        [EloquentInterface]
        public interface IContract
        {
            [EloquentEvent]
            event EventHandler RegularEvent;

            [EloquentEvent]
            event EventHandler<CustomEventArgs> RegularEventWithArgs;

            [EloquentEvent]
            event Action NoParameterEvent;

            [EloquentEvent]
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


# TODO

## Features
1. Security
2. Polling mode
3. Client event that connection is lost. Restore connection.

## Improvements
1. Default parameters for interfaces
2. Proto nuget
3. Benchmark: gRPC
4. Benchmark: .NET Remoting
5. Send only subscribed events
6. Raise exception if contract has method, property or event without attribute.
