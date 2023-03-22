<Query Kind="Statements">
  <Output>DataGrids</Output>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

 static async Task Method()
{
	string key = "code";
	var wordMap = new ConcurrentDictionary<string, Lazy<int>>();

	Func<string, int,int, int> updateValueFactory = (key, currentValue,taskId) =>
	{
		Console.WriteLine($"taskid={taskId} currentValue={currentValue}");

		return currentValue + 1;
	};

	var allTasks = new List<Task>();

	for (int i = 0; i < 1000; i++)
	{
		int taskId = i;

		allTasks.Add(Task.Run(() =>
		{			
			wordMap.AddOrUpdate(key, new Lazy<int>(() => 0), (key, currentValue) => new Lazy<int>(() => updateValueFactory(key, currentValue.Value, taskId)));

			wordMap.AddOrUpdateL(key, key => 0, updateValueFactory: (key, currentValue) =>
			{
				Console.WriteLine($"taskid={taskId} currentValue={currentValue}");

				return currentValue + 1;
			});

		}));
	}
	await Task.WhenAll(allTasks);
	Console.WriteLine($"Final value={wordMap["code"]}");

}

 Method();

    
public static class ExtDictionary
{
	public static TValue AddOrUpdateL<TKey, TValue>(this ConcurrentDictionary<TKey, Lazy<TValue>> dictionary, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
	{
		if (dictionary == null) throw new ArgumentNullException("dictionary");
		var result = dictionary.AddOrUpdate(key, new Lazy<TValue>(() => addValueFactory(key)), (key2, old) => new Lazy<TValue>(() => updateValueFactory(key2, old.Value)));
		return result.Value;
	}

}

//do
//{
//	while (!TryGetValue(key, out comparisonValue))
//	{
//		TValue obj = addValueFactory(key);
//		TValue resultingValue;
//		if (TryAddInternal(key, obj, false, true, out resultingValue))
//			return resultingValue;
//	}
//	newValue = updateValueFactory(key, comparisonValue);
//}
//while (!TryUpdate(key, newValue, comparisonValue));
//return newValue;


//taskid = 0 currentValue = 0
//taskid = 1 currentValue = 1
//taskid = 1 currentValue = 2
//taskid = 2 currentValue = 3
//taskid = 2 currentValue = 4
//taskid = 4 currentValue = 5
//taskid = 4 currentValue = 6
//taskid = 3 currentValue = 7
//taskid = 3 currentValue = 8
//taskid = 5 currentValue = 9
//taskid = 6 currentValue = 10
//taskid = 6 currentValue = 11
//taskid = 7 currentValue = 12
//taskid = 5 currentValue = 13
//taskid = 7 currentValue = 14
//taskid = 8 currentValue = 15
//taskid = 9 currentValue = 16
//taskid = 8 currentValue = 17
//taskid = 9 currentValue = 18
//taskid = 10 currentValue = 19
//taskid = 11 currentValue = 20
//taskid = 10 currentValue = 21
//taskid = 12 currentValue = 22
//taskid = 11 currentValue = 23
//taskid = 13 currentValue = 24
//taskid = 12 currentValue = 25
//taskid = 13 currentValue = 26
