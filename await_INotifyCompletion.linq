static public Action Tick = null;

void Main()
{
	Start();
	while (true)
	{
		if (Tick != null) Tick();
		Thread.Sleep(1);
	}
}

async void Start()
{
	Console.WriteLine("Start execution");
	for (int i = 1; i <= 4; ++i)
	{
		Console.WriteLine($"The {i}th time, time: {DateTime.Now.ToString("HH:mm:ss")} - Thread number: {Thread.CurrentThread.ManagedThreadId}");
		await TaskEx.Delay(1000);
	}
	Console.WriteLine("Execution completed");
}

class TaskEx
{
	public static MyDelay Delay(int ms) => new MyDelay(ms);
}

class MyDelay : INotifyCompletion
{
	private readonly double _start;
	private readonly int _ms;

	public MyDelay(int ms)
	{
		_start = Util.ElapsedTime.TotalMilliseconds;
		_ms = ms;
	}

	internal MyDelay GetAwaiter() => this;

	public void OnCompleted(Action continuation)
	{
		Tick += Check;

		void Check()
		{
			if (Util.ElapsedTime.TotalMilliseconds - _start > _ms)
			{
				continuation();
				Tick -= Check;
			}
		}
	}

	public void GetResult() { }

	public bool IsCompleted => false;
}


//The execution is suspended afterward, and returns to the state machine after OnCompleted is completed;