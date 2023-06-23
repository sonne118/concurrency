
    public class ObjectPool<T> where T : class
    {
        private T firstItem;
        private readonly T[] items;
        private readonly Func<T> generator;

        public ObjectPool(Func<T> generator, int size)
        {
            this.generator = generator ?? throw new ArgumentNullException("generator");
            this.items = new T[size - 1];
        }

        public T Rent()
        {
            Console.WriteLine("R:");
            T inst = firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref firstItem, null, inst))
            {
                inst = RentSlow();
            }
            return inst;
        }

        public void Return(T item)
        {
            Console.WriteLine("*");
            if (firstItem == null)
            {

                firstItem = item;
            }
            else
            {
                ReturnSlow(item);
            }
        }

        private T RentSlow()
        {
            for (int i = 0; i < items.Length; i++)
            {
                T inst = items[i];
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i], null, inst))
                    {
                        Console.WriteLine("  -");
                        return inst;
                    }
                }
            }
            Console.WriteLine("  --");
            return generator();
        }

        private void ReturnSlow(T obj)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = obj;
                    break;
                }
            }
        }
    }

