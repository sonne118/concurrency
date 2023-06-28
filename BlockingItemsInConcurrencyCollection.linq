internal class Program
{
	static void Main()
	{
		var w = new WaiterApi();
		w.Ids = new int[] { 10, 30, 100 };
		//w.ControllerMethod(true);
	}
}

public class Tmp
{
	private WaitHandle waitHandle = default;
	public int Prop1 { get; set; }
	public AutoResetEvent Prop2 { get; set; }

	public Tmp(int _prop1, WaitHandle _waitHandle)
	{
		this.Prop1 = _prop1;
		this.Prop2 = _waitHandle as AutoResetEvent;
	}
}

public class WaiterApi
{
	private int[] ids;
	public int[] Ids { get { return ids; } set { ids = value; } }

	private readonly ConcurrentDictionary<int, Tmp> d = new();

	List<WaitHandle> waitHandles = new List<WaitHandle> {
							   new AutoResetEvent(false),
							   new AutoResetEvent(false),
							   new AutoResetEvent(false)
							   };
	public WaiterApi()
	{
		Task.Run(() => ControllerMethod(true));
		Task.Run(() => ControllerMethod(false));
		Task.Run(() => ControllerMethod(false));
	}

	public Task ControllerMethod(bool flag)
	{
		if (flag)
		{
			for (int c = 0; c < ids.Length; c++)
			{
				d.TryAdd(c, new Tmp(ids[c], waitHandles[c]));
			}
		}

		Task.Factory.StartNew(async () => await Worker(ids));
		WaitHandle.WaitAll(waitHandles.ToArray());
		Console.WriteLine("-------");

		for (int j = 0; j < d.Count(); j++)
		{
			Console.WriteLine(d[j].Prop1);
		}
		return Task.CompletedTask;
	}

	public async Task Worker(int[] ids)
	{
		Tmp[] tasks = default; int k = 0;
		HashSet<Task<Tmp>> _asyncWaiters = new();

		for (int n = 0; n < ids.Length; n++)
		{
			int r = n;
			if (d.TryGetValue(n, out Tmp v))
			{
				_asyncWaiters.Add(Task.Factory.StartNew<Task<Tmp>>(async () =>
				{
					Tmp upd = await Do(v);
					if (d.TryUpdate(r, upd, v))
					{
						return upd;
					}
					return v;
				}
				).Unwrap());
			}
		}
		
		try
		{
			tasks = await Task.WhenAll(_asyncWaiters);
		}
		catch (AggregateException ae)
		{
			foreach (var e in ae.InnerExceptions)
			{
				System.Diagnostics.Debug.WriteLine("{0}:\n   {1}", e.GetType().Name, e.Message);
			}
		}
		finally
		{
			if (tasks.Length > 0)
			{
				foreach (var item in tasks)
				{
					item.Prop2.Set();
					Interlocked.Increment(ref k);
				}
			}
		}

		if (k == ids.Length)
			await Task.CompletedTask;
		else
			await Task.FromException(new Exception());
	}

	private async Task<Tmp> Do(Tmp v)
	{
		if (v == null)
			return await Task.FromException<Tmp>(new Exception()); ;

		Tmp updated = default;
		updated = v;
		updated.Prop1 = updated.Prop1 + 1000;

		return await Task.FromResult(updated);
	}
}






