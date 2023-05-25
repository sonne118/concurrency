<Query Kind="Program">
  <Output>DataGrids</Output>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static void Main(string[] args)
{
	var IncrementValue = 0;
	Parallel.For(0, 100000, _ =>
	{
		//Incrementing the value
		Interlocked.Increment(ref IncrementValue);
	});
	Console.WriteLine("Expected Result: 100000");
	Console.WriteLine($"Actual Result: {IncrementValue}");
	//Console.ReadKey();
}