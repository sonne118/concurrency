
	class Program
	{
		private static long _value = long.MaxValue;
		private static int _resourceInUse = 0;

		static void Main(string[] args)
		{
			CompareExchangeVariables();
			AddVariables();
			DecrementVariable();
			IncrementVariable();
			ReadVariable();
			PerformUnsafeCodeSafely();
		}

		private static void CompareExchangeVariables()
		{
			Interlocked.CompareExchange(ref _value, 123, long.MaxValue);
		}

		private static void AddVariables()
		{
			Interlocked.Add(ref _value, 321);
		}

		private static void DecrementVariable()
		{
			Interlocked.Decrement(ref _value);
		}

		private static void IncrementVariable()
		{
			Interlocked.Increment(ref _value);
		}

		private static long ReadVariable()
		{			
			return Interlocked.Read(ref _value);
		}

		private static void PerformUnsafeCodeSafely()
		{
			for (int i = 0; i < 5; i++)
			{
				UseResource();
				Thread.Sleep(1000);
			}
		}
		
		static bool UseResource()
		{
			//0 indicates that the method is not in use.
			if (0 == Interlocked.Exchange(ref _resourceInUse, 1))
			{
				Console.WriteLine($"{Thread.CurrentThread.Name} acquired the lock");				
				
				NonThreadSafeResourceAccess();

				//Simulate some work
				Thread.Sleep(500);

				Console.WriteLine($"{Thread.CurrentThread.Name} exiting lock");

				//Release the lock
				Interlocked.Exchange(ref _resourceInUse, 0);
				return true;
			}
			else
			{
				Console.WriteLine($"{Thread.CurrentThread.Name} was denied the lock");
				return false;
			}
		}

		private static void NonThreadSafeResourceAccess()
		{
			Console.WriteLine("Non-thread-safe code executed.");
		}
	}

