private static ParserActor actor;
private static SumActor sumActor;
private static EventBus eventbus;

 async Task Main(string[] args)
{
	eventbus = new EventBus();
	actor = new ParserActor(eventbus);
	sumActor = new SumActor();

	eventbus.Subscribe<OnTopicParsed>(HandleOnTopicParsed);


	await sumActor.SendMessage(1);
	await sumActor.SendMessage(1);


	await Task.Delay(1);
	var state = sumActor.GetState();

	await DoParse();

	Console.ReadLine();
}

private static Task HandleOnTopicParsed(OnTopicParsed arg)
{
	Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] Parsed: {arg.Topic.Title} - {arg.Topic.Url}");

	return Task.CompletedTask;
}

private static async Task DoParse()
{
	var parser = new HabrParser();

	await foreach (var url in parser.GetPages())
	{
		await actor.SendMessage(url);
	}
}

	public abstract class AbstractActor<T>
	{
		private readonly BufferBlock<T> _mailBox;
		private readonly List<Task> _activeWorkers;

		protected abstract int ThreadCount { get; }

		public AbstractActor()
		{
			_mailBox = new BufferBlock<T>();
			_activeWorkers = new List<Task>();

			Task.Run(async () =>
			{
				while (true)
				{
					while (_activeWorkers.Count < ThreadCount)
					{
						_activeWorkers.Add(Worker());
					}

					await Task.WhenAny(_activeWorkers);
					_activeWorkers.RemoveAll(s => s.IsCompleted);
				}
			});
		}

		private async Task Worker()
		{
			var message = await _mailBox.ReceiveAsync();
			try
			{
				await HandleItem(message);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		protected abstract Task HandleItem(T message);

		public Task SendMessage(T message) => _mailBox.SendAsync(message);
		public void ClearQueue() => _mailBox.TryReceiveAll(out var _);
	}

public class ParserActor : AbstractActor<string>
{
	private readonly HabrParser _parser;
	private readonly EventBus _eventBus;

	protected override int ThreadCount => 10;

	public ParserActor(EventBus eventBus)
	{
		_parser = new HabrParser();
		_eventBus = eventBus;
	}

	protected override async Task HandleItem(string message)
	{
		try
		{
			var topics = await _parser.ParseTopics(message);

			foreach (var item in topics)
			{
				_ = _eventBus.Publish(new OnTopicParsed(item));
			}
		}
		catch (Exception ex)
		{
			await SendMessage(message);
		}
	}

}

public class OnTopicParsed : IEvent
{
	public OnTopicParsed(Topic topic)
	{
		Topic = topic;
	}

	public Topic Topic { get; set; }
}

public interface IEvent { }

public class EventBus
{
	private ConcurrentDictionary<EventSubscriber, Func<IEvent, Task>> _subscibers;

	public EventBus()
	{
		_subscibers = new ConcurrentDictionary<EventSubscriber, Func<IEvent, Task>>();
	}

	public IDisposable Subscribe<T>(Func<T, Task> handler) where T : IEvent
	{
		var disposer = new EventSubscriber(typeof(T), s => _subscibers.TryRemove(s, out var _));

		_subscibers.TryAdd(disposer, (item) => handler((T)item));

		return disposer;
	}

	public async Task Publish<T>(T message) where T : IEvent
	{
		var messageType = typeof(T);

		var handlers = _subscibers
			.Where(s => s.Key.MessageType == messageType)
			.Select(s => s.Value(message));

		await Task.WhenAll(handlers);
	}
}

class EventSubscriber : IDisposable
{
	private readonly Action<EventSubscriber> _action;

	public Type MessageType { get; }

	public EventSubscriber(Type messageType, Action<EventSubscriber> action)
	{
		MessageType = messageType;
		_action = action;
	}

	public void Dispose()
	{
		_action?.Invoke(this);
	}
}

public class Topic
{
	public string Title { get; set; }
	public string Url { get; set; }
}

public class HabrParser
{
	private readonly HttpClient _http;

	public HabrParser()
	{
		_http = new HttpClient();
	}


	public async IAsyncEnumerable<string> GetPages()
	{
		await Task.Delay(100);
		yield return "https://habr.com/ru/all/";
		yield return "https://habr.com/ru/all/page2/";
		yield return "https://habr.com/ru/all/page3/";
		yield return "https://habr.com/ru/all/page4/";
		yield return "https://habr.com/ru/all/page5/";
		yield return "https://habr.com/ru/all/page6/";
		yield return "https://habr.com/ru/all/page7/";
		yield return "https://habr.com/ru/all/page8/";
		yield return "https://habr.com/ru/all/page9/";
		yield return "https://habr.com/ru/all/page10/";
	}

	public async Task<Topic[]> ParseTopics(string url)
	{
		var res = new List<Topic>();
		var body = await _http.GetStringAsync(url);

		var titles = Regex.Matches(body, "(?<=class=\"post__title_link\">)[^</a>]+");
		var urls = Regex.Matches(body, "(?<=<a href=\")[^\"]+(?=\" class=\"post__title_link\">)");

		for (int i = 0; i < titles.Count; i++)
		{
			res.Add(new Topic
			{
				Title = titles[i].Value,
				Url = urls[i].Value,
			});
		}

		return res.ToArray();
	}
}

public class SumActor : AbstractActor<int>
{
	protected override int ThreadCount => 1;


	private int State { get; set; }

	protected override Task HandleItem(int message)
	{
		State += message;

		return Task.CompletedTask;
	}

	public int GetState() => State;
}
