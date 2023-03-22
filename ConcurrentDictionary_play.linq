 static void WorkConcurrentDictionary()
{
	var players = new ConcurrentDictionary<string, int>();
	const int playerLimit = 10;

	// creating new players each 2 seconds
	var createPlayerTask = Task.Factory.StartNew(() =>
	{
		var index = 1;
		while (true)
		{
			var playerName = $"player_{index}";

			if (!players.ContainsKey(playerName))
			{
				players.TryAdd(playerName, 0);
			}

			Thread.Sleep(2000);
			index++;

			if (index > playerLimit)
			{
				index = 1;
			}
		}
	});

	// update players score
	var updateScoreTask = Task.Factory.StartNew(() =>
	{
		var random = new Random();
		var index = 1;

		while (true)
		{
			var playerName = $"player_{index}";
			players.TryGetValue(playerName, out var score);

			Thread.Sleep(100);
			players.TryUpdate(playerName, random.Next(1, 10), score);

			index++;

			if (index > playerLimit)
			{
				index = 1;
			}
		}
	});

	// remove random players each 2 seconds
	var removePlayerTask = Task.Factory.StartNew(() =>
	{
		var random = new Random();

		while (true)
		{
			var index = random.Next(1, (playerLimit + 1));
			var playerName = $"player_{index}";

			players.TryRemove(playerName, out var score);

			Thread.Sleep(2000);
		}
	});

	// display players score
	var showScoreTask = Task.Factory.StartNew(() =>
	{
		while (true)
		{
			Console.Clear();
			var output = new StringBuilder();

			output.AppendLine("Score ====");

			foreach (var (playerName, playerScore) in players)
			{
				output.AppendLine($"{playerName}: {playerScore}");
			}

			Console.WriteLine(output);
			Thread.Sleep(1000);
		}
	});

	Task.WhenAll(createPlayerTask, updateScoreTask, showScoreTask, removePlayerTask);

	Console.ReadLine();
}

WorkConcurrentDictionary();
