﻿using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PLINQTask
{
    internal class Program
    {
        static void PerfectNumbersWithPar() // решение влоб
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<int> array = (
                from element in Enumerable.Range(2, (int)Math.Pow(10, 6)).AsParallel()
                                                                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                                                                        .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                                                        .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                where element % 2 == 0 && (from devider in Enumerable.Range(1, element - 1).AsParallel()
                                                                                                .WithDegreeOfParallelism(Environment.ProcessorCount)
                                                                                                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                                                                                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                                           where element % devider == 0
                                           select devider).Sum() == element
                select element
                          )
                            .OrderBy(a => a)
                            .ToList();
            sw.Stop();
            array.ForEach(a => Console.WriteLine(a));
            Console.WriteLine(sw.Elapsed.TotalSeconds);
        }
        static void PerfectNumbersWithParClever() // решение по-умному
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<int> array = (
            from p in Enumerable.Range(2, 11).AsParallel()
                                            .WithDegreeOfParallelism(Environment.ProcessorCount)
                                            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                            .WithMergeOptions(ParallelMergeOptions.NotBuffered)
            where Enumerable.Range(2, (int)Math.Sqrt(Math.Pow(2, p) - 1)).AsParallel()
                                                                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                                                                        .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                                                        .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                                                                        .All(a => (int)(Math.Pow(2, p) - 1) % a != 0)
            select (int)(Math.Pow(2, p - 1) * (Math.Pow(2, p) - 1))
                          )
                            .OrderBy(a => a)
                            .ToList();
            sw.Stop();
            array.ForEach(a => Console.WriteLine(a));
            Console.WriteLine(sw.Elapsed.TotalSeconds);
        }
        static void PerfectNumbersWithoutParClever() // решение по-умному, но без параллельности
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<int> array = (
            from p in Enumerable.Range(2, 11)
            where Enumerable.Range(2, (int)Math.Sqrt(Math.Pow(2, p) - 1))
                                                                        .All(a => (int)(Math.Pow(2, p) - 1) % a != 0)
            select (int)(Math.Pow(2, p - 1) * (Math.Pow(2, p) - 1))
                          )
                            .OrderBy(a => a)
                            .ToList();
            sw.Stop();
            array.ForEach(a => Console.WriteLine(a));
            Console.WriteLine(sw.Elapsed.TotalSeconds);
        }
        static void Main(string[] args)
        {
            // совершенное число - это такое, которое равно сумме всех своих натуральных делителей. Умные математики доказали, что 
            // 2^(p - 1)(2^p - 1) - совершенное, если 2^p - 1 - простое число, более того, все совершенные числа до 
            // 10^(1500) представимы в таком виде (это простая информация из википедии для облегчения решения задачи)
            // можно вычислить, что максимальное p, которое нам понадобится - это p = 12
            char[] delimiters = new char[] { '.', ',', ';', '\'', '-', ':', '!', '?', '(', ')', '<', '>', '=', '*', '/', '[', ']', '{', '}', '\\', '"', '\r', '\n' };

            using (StreamReader stream = new StreamReader(@"C:\Users\fiska\source\repos\PLINQTask\PLINQTask\Война и мир.txt"))

            {
                string[] arrayOfWords = (from str in delimiters.Aggregate(stream.ReadToEnd(), (ch1, ch2) => ch1.Replace(ch2, ' '))
                                                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                                .AsParallel()
                                        select str.Replace("– ", "").ToLower())
                                                                     .ToArray();

                
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Dictionary< string, int> WordsCounter = (from word in arrayOfWords.Distinct()
                                                                                    .AsParallel()
                                                        where word.Length >= 5 
                                                        select (arrayOfWords.Count(a => a == word), word)).OrderByDescending(a => a.Item1)
                                                                                                            .AsParallel()
                                                                                                            .ToDictionary(a => a.Item2, a => a.Item1);
                sw.Stop();

                WordsCounter.Keys.Take(10).ToList().ForEach(a => Console.WriteLine($"{a} входит {WordsCounter[a]} раз"));
                Console.WriteLine($"Это дело выполнялось аж {sw.Elapsed.TotalSeconds} секунд");
            }
        }
    }
}