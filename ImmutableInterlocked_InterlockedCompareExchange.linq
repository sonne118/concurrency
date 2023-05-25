
ImmutableArray<int> arr =  ImmutableArray<int>.Empty;

//string [] arr = new string [50000];

Task [] tasks = new Task[50];
for (int i = 0; i < 50; i++)
{
	tasks[i] = Task.Run(() =>
	{
		for (int j = 0; j < 1000; j++)
		{
			Add(j);
		}
	}
);	
}
await Task.WhenAll(tasks).ConfigureAwait(false);
Console.WriteLine(arr.Length);

void Add(int a)
{			
	 var action = arr;
	 while (true)
	{		
		var local = action;
		var value = arr.Add(a);

	   action= ImmutableInterlocked.InterlockedCompareExchange(ref  arr , value, local);		   	
	   //action= Interlocked.CompareExchange<string>(ref arr.l, v, local);	
     if (local == action)
	  break;		  
	};	
}

