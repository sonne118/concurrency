private static int _runCount = 0;
private static readonly ConcurrentDictionary<string, Lazy<string>> _lazyDictionary
	= new ConcurrentDictionary<string, Lazy<string>>();

public static void Main(string[] args)
{
	var task1 = Task.Run(() => PrintValueLazy("The first value"));
	Task.Delay(3000);
	var task2 = Task.Run(() => PrintValueLazy("The second value"));
	Task.WaitAll(task1, task2);

	PrintValueLazy("The third value");


	Console.WriteLine($"Run count: {_runCount}");
}

public static void PrintValueLazy(string valueToPrint)
{
	var valueFound = _lazyDictionary.GetOrAdd("key",
				x => new Lazy<string>(
					() =>
						{
							Interlocked.Increment(ref _runCount);
							Thread.Sleep(100);
							return valueToPrint;
						}));
	Console.WriteLine(valueFound.Value);
}