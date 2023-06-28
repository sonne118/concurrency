<Query Kind="Program">
  <Output>DataGrids</Output>
  <NuGetReference>App.Metrics.AspNetCore</NuGetReference>
  <NuGetReference>App.Metrics.AspNetCore.Core</NuGetReference>
  <NuGetReference>Microsoft.AspNetCore.Http</NuGetReference>
  <NuGetReference>Microsoft.AspNetCore.Http.Features</NuGetReference>
  <NuGetReference>Microsoft.AspNetCore.Mvc</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Configuration</NuGetReference>
  <NuGetReference>Microsoft.Extensions.DependencyInjection</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Http.Polly</NuGetReference>
  <NuGetReference>NUnitLite</NuGetReference>
  <NuGetReference>System.Threading.Tasks.Dataflow</NuGetReference>
  <Namespace>Microsoft.AspNetCore.Builder</Namespace>
  <Namespace>Microsoft.Extensions.Hosting</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Collections.Immutable</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>


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

			var t = Task.Factory.StartNew(async () => await Worker(ids));
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
				return await Task.FromResult<Tmp>(null);

			Tmp updated = default;
			updated = v;
			updated.Prop1 = updated.Prop1 + 1000;

			return await Task.FromResult(updated);
		}
	}






