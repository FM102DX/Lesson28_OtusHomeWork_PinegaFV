using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Lesson28_MultiThread_PinegaFV
{
    public static class ConsoleWriter
    {
        //класс, который пишет в консоль разными цветами и пр.
        private static readonly object _locker = new object();
        public static void WriteWithColor(string text, ConsoleColor color)
        {
            lock (_locker) //если не использовать локер, цвета смешиваются
            {
                ConsoleColor _color = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.WriteLine("");
                Console.ForegroundColor = _color;
            }
        }

        public static void WriteDefault(string text) { WriteWithColor(text, Console.ForegroundColor); }

        public static void WriteRed(string text) { WriteWithColor(text, ConsoleColor.Red); }

        public static void WriteGreen(string text) { WriteWithColor(text, ConsoleColor.Green); }

        public static void WriteYellow(string text) { WriteWithColor(text, ConsoleColor.Yellow); }

    }

    public class FileWriter
    {
        StreamWriter _file;
        
        public FileWriter(string path)
        {
            _file= File.CreateText(path);
        }

        public void DoWrite(string text)
        {
            _file.WriteLine(text);
        }

        public void Close()
        {
            _file.Flush();
            _file.Close();
        }

    }

    public class MyResult
    {
        public long rezult;
        public long elapsedMs;
        public static MyResult GetInstance(long rezultVar, long elapsedMsVar) 
        {
            return new MyResult
            {
                elapsedMs = elapsedMsVar,
                rezult = rezultVar
            };
        }
    }

    public class ChunkSplitter
    {
        public static List<IEnumerable<int>> SplitToChunks(IEnumerable<int> data, int chunksCount)
        {
            var partSize = data.Count() / chunksCount;

            var chunks = new List<IEnumerable<int>>();

            for (int i = 0; i < chunksCount; i++)
            {
                var pool = data.Skip(partSize * i);
                var chunk = i < chunksCount - 1
                    ? pool.Take(partSize).ToList()
                    : pool.ToList();

                chunks.Add(chunk);
            }

            return chunks;
        }
    }

}
