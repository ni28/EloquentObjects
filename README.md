# EloquentObjects
EloquentObjects is a .NET object-oriented Inter-Process Communication (IPC) and Remote-Procedure Call (RPC) framework that allows clients to work with hosted objects remotelly (call methods, get or set properties, subscribe to events, etc.).

# Concepts
EloquentObjects can host objects that implement attributed interfaces (like in WCF) using TCP (for RPC) or named pipes (for IPC) bindings.
Multiple clients can connect to the same hosted object remotelly. Each hosted object has an object ID that is used by clients to distinguish between hosted objects.

Following features are provided by hosted objects:
	Client can call methods of hosted object and get responses.
	Client receives and rethrow exception if it occured in hosted object method
	Client can call one-way methods (responses and exceptions are not sent to client).
	Client can subscribe to hosted objects events.

Note that EloquentObjects behave differently from traditional Remote Procedure Call (RPC) implementations, for example:
    In RPC, the client makes a request and waits for the response.
    In RPC, the server doesn't push anything to the client unless it's in response to a request.
    Often, the design of RPC is such that different clients are independent of each other.

# Quick start example
1. Create Server, Client and Contract assemblies
2. Define an attributed contract in a Contract assembly that is available both for server and client:

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
        event Action<string, OperationResult> ResultReady;
	}
	
3. Implement a contract in Server assembly.

    internal sealed class EloquentCalculator: IEloquentCalculator
    {
        #region Implementation of ICalculatorService
		...
        #endregion
    }
}

4. Create a server and start an object hosting in a Server assembly:
	
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

5. Connect to a hosted object from Client assembly:

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
	
	
# TODO
0. Self as sender
1. Named pipes binding
2. Default parameters
3. Proto nuget
4. Benchmark: gRPC
5. Benchmark: .NET Remoting
6. Polling mode

