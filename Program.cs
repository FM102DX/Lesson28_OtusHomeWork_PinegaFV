using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace Lesson28_MultiThread_PinegaFV
{
    class Program
    {
        private static long _numbersCol = 10_000_000; //количество элементов массива
        private static int[] _array = new int[_numbersCol];
        private static int _minValue = 0;
        private static int _maxValue = 99;
        private static int _threadCount = 10;

        static void Main(string[] args)
        {
            Console.WriteLine($"Создаем массив из {_numbersCol} элементов...");
            long elapsedTime = FillTheArray();
            Console.WriteLine($"Готово, потрачено {elapsedTime} ms");
            MyResult rez;
            do
            {
                int menuPoint = ShowMenu();
                switch (menuPoint)
                {
                    case 1:
                        rez = GetGenericSum();
                        Console.WriteLine($"Суммируем {_numbersCol} элементов простым перечислением, результат={rez.rezult}, время={rez.elapsedMs} ms");
                        break;

                    case 2:

                        Console.WriteLine($"Суммируем {_numbersCol} элементов несколькими потоками, начинаем");
                        rez = GetParallelSum();
                        Console.WriteLine($"Результат={rez.rezult}, время={rez.elapsedMs} ms");
                        break;

                    case 3:
                        Console.WriteLine($"Суммируем {_numbersCol} элементов при помощи Linq, начинаем");
                        rez = GetSumByLinq();
                        Console.WriteLine($"Результат={rez.rezult}, время={rez.elapsedMs} ms");

                        break;

                    case 9:
                        return;

                    default:

                        break;
                }

            }
            while (true);
        }

        static int ShowMenu()
        {
            bool correctInput = false;
            int rez = 0;

            do
            {
                Console.WriteLine("");
                Console.WriteLine("Выберите одно из действий (введите число):");
                Console.WriteLine("1 - Суммируем простым перечислением");
                Console.WriteLine("2 - Суммируем несколькими потоками");
                Console.WriteLine("3 - Суммируем при помощи Linq");

                Console.WriteLine("9 - выход");
                Console.WriteLine("");

                string key = Console.ReadLine().Trim();

                if ((key == "1" || key == "2" || key == "3" || key == "4" || key == "5" || key == "6" || key == "7" || key == "8" || key == "9") && (Int32.TryParse(key, out rez)))
                {
                    correctInput = true;
                }
            }
            while (!correctInput);

            return rez;
        }

        static MyResult GetGenericSum()
        {
            var sw = new Stopwatch();

            sw.Start();

            long result = 0;

            foreach (long item in _array)
            {
                result += item;
            }

            sw.Stop();

            return MyResult.GetInstance(result, sw.ElapsedMilliseconds);
        }

        static long FillTheArray()
        {
            var rnd = new Random();

            var sw = new Stopwatch();

            sw.Start();

            for (long i = 0; i < _array.Length; i++)
            {
                _array[i] = rnd.Next(_minValue, _maxValue);
            }
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }

        public static MyResult GetParallelSum()
        {
            var partSize = _array.Length / _threadCount;

            List<int> numList = _array.ToList();

            long TotalSum = 0;

            List<MyTreadClass> threadPool = new List<MyTreadClass>();

            var stopEvents = new WaitHandle[_threadCount];

            var sw = new Stopwatch();

            Console.WriteLine($"Starting data load with {_threadCount} threads");

            sw.Start();

            var chunks = ChunkSplitter.SplitToChunks(numList, _threadCount);

            int counter = 0;

            foreach (var chunk in chunks)
            {
                var autoResetEvent = new AutoResetEvent(false);

                threadPool.Add(new MyTreadClass(counter, chunk, autoResetEvent));

                stopEvents[counter] = autoResetEvent;

                counter++;
            }

            ConsoleWriter.WriteDefault("All treads started");

            WaitHandle.WaitAll(stopEvents);
            //TODO всe используют foreach(var thread in threads) {thread.Join();}

            threadPool.ForEach(x => TotalSum += x.TotalSum);

            sw.Stop();

            return MyResult.GetInstance(TotalSum, sw.ElapsedMilliseconds);

        }

        public class MyTreadClass
        {
            //сделан для того, чтобы каждый тред был инкапсулирован своим потоком
            Thread _myThread;

            public long TotalSum = 0;

            public MyTreadClass(int i, IEnumerable<int> chunk, AutoResetEvent autoResetEvent)
            {
                bool success = false;
                do
                {
                    try
                    {
                        _myThread = new Thread(x => CountChunk(chunk, autoResetEvent, i));
                        _myThread.Name = "trName" + i.ToString();
                        _myThread.Start();
                        success = true;
                    }
                    catch
                    {
                    }
                }
                while (!success);
            }

            private void CountChunk(IEnumerable<int> chunk, AutoResetEvent autoResetEvent, int threadNo)
            {
                ConsoleWriter.WriteDefault($"Thread №={threadNo} ManagedThreadId={Thread.CurrentThread.ManagedThreadId} trName = {Thread.CurrentThread.Name} started counting {chunk.Count()} objects");
                var sw = new Stopwatch();
                sw.Start();
                
                long result = 0;

                foreach (int item in chunk)
                {
                    result += item;
                }
                TotalSum = result;
                sw.Stop();
                ConsoleWriter.WriteDefault($"Thread №={threadNo} ManagedThreadId={Thread.CurrentThread.ManagedThreadId} finished, elapsed time: {sw.ElapsedMilliseconds} ms");
                autoResetEvent.Set();
            }
        }

        public static MyResult GetSumByLinq()
        {
            var sw = new Stopwatch();

            Console.WriteLine($"Starting data load with {_threadCount} threads");

            sw.Start();

            var result = _array.AsParallel().Sum();

            sw.Stop();

            return MyResult.GetInstance(result, sw.ElapsedMilliseconds);
        }
    }
}
