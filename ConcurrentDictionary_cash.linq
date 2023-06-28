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
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

	async Task Main()
	{
		var urls = new List<string>();
	
		urls.Add("https://www.cnn.com");
		urls.Add("https://www.bbc.com");
		urls.Add("https://www.amazon.com");
		urls.Add("https://www.cnn.com");
		urls.Add("https://www.bbc.com");
	
	
		var downloadUrl = MemoizeLazy<string, string>(async (url) =>
			  {
				  $"processing url {url}".Dump();
				  using (var wc = new WebClient())
					  return await wc.DownloadStringTaskAsync(url);
			  });
	
		foreach (var url in urls)
		{
			var result = await downloadUrl(url);
			Console.WriteLine($"Url {url} length {result.Length}");
		}
	}
	
	Func<T, Task<R>> MemoizeLazy<T, R>(Func<T, Task<R>> func) where T : IComparable
	{
		ConcurrentDictionary<T, Lazy<Task<R>>> cache = new ConcurrentDictionary<T, Lazy<Task<R>>>();
		return arg => cache.GetOrAdd(arg, a => new Lazy<Task<R>>(() => func(a))).Value;
	}
	
	


// You can define other methods, fields, classes and namespaces here