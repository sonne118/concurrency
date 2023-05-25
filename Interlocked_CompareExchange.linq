
ImmutableArray<int> arr =  ImmutableArray<int>.Empty;
string[] arr = new string[5000];

Task[] tasks = new Task[50];
for (int i = 0; i < 50; i++)
{
	tasks[i] = Task.Run(() =>
	{
		for (int j = 0; j < 100; j++)
		{
			Add(j.ToString());
		}
	}
);
}
await Task.WhenAll(tasks).ConfigureAwait(false);
Console.WriteLine(arr.Length);

void Add(string a)
{
	var action = arr;
	while (true)
	{
		var local = action;
		// local[arr.Length+1] =a; 
		//var v = local[arr.Length+1];
		var v = arr.Append(a).ToArray();	
		var n = arr.Length - 1;			   	
		action= Interlocked.CompareExchange<string[] >(ref arr, v, local);
		if (local == action)
			break;
	};
}

