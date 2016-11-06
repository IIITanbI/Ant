using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsehan
{
    public static class Extentsion
    {
        public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
            {
                yield break;
            }

            var list = sequence.ToList();

            if (!list.Any())
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                var startingElementIndex = 0;

                foreach (var startingElement in list)
                {
                    var remainingItems = list.AllExcept(startingElementIndex);

                    foreach (var permutationOfRemainder in remainingItems.Permute())
                    {
                        yield return startingElement.Concat(permutationOfRemainder);
                    }

                    startingElementIndex++;
                }
            }
        }

        private static IEnumerable<T> Concat<T>(this T firstElement, IEnumerable<T> secondSequence)
        {
            yield return firstElement;
            if (secondSequence == null)
            {
                yield break;
            }

            foreach (var item in secondSequence)
            {
                yield return item;
            }
        }

        private static IEnumerable<T> AllExcept<T>(this IEnumerable<T> sequence, int indexToSkip)
        {
            if (sequence == null)
            {
                yield break;
            }

            var index = 0;

            foreach (var item in sequence.Where(item => index++ != indexToSkip))
            {
                yield return item;
            }
        }
    }

    struct TimeInterval : IComparable<TimeInterval>
    {
        public int Start { get; set; }
        public int End { get; set; }
        public TimeInterval(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        public int CompareTo(TimeInterval b)
        {
            int cmp = Start.CompareTo(b.Start);
            if (cmp != 0)
                return cmp;
            return End.CompareTo(b.End);
        }
    }
    class Solution : IComparable<Solution>
    {
        public int AllTime { get; set; }
        public int Downtime { get; set; }
        public List<int> DowntimeList { get; set; }
        public List<Task> Tasks { get; set; }

        public int CompareTo(Solution b)
        {
            int cmp = AllTime.CompareTo(b.AllTime);
            if (cmp != 0)
                return cmp;
            return Downtime.CompareTo(b.Downtime);
        }
    }
    class Machine
    {
        class MacnhineItem
        {
            public Task Task { get; set; } = null;
            public int EndTime { get; set; } = -1;
            public int ArrivalTime { get; set; } = -1;
        }
        Queue<MacnhineItem> q = new Queue<MacnhineItem>();
        public List<TimeInterval> DowntimeList = new List<TimeInterval>();

        public bool IsEmpty => q.Count == 0;
        public int EndTime => q.Count > 0 ? q.Peek().EndTime : -1;
        private int _lastTime;

        public Machine()
        {
            Reset();
        }
        public void AddTask(Task task, int time)
        {
            var temp = new MacnhineItem();
            temp.Task = task;
            temp.ArrivalTime = time;

            if (IsEmpty)
            {
                temp.EndTime = temp.ArrivalTime + task[this];

                if (_lastTime != time)
                    DowntimeList.Add(new TimeInterval(_lastTime, time));
            }

            q.Enqueue(temp);
        }
        public Task RemoveCurrentTask()
        {
            MacnhineItem temp = q.Dequeue();
            Task task = temp.Task;
            _lastTime = temp.EndTime;

            if (!IsEmpty)
            {
                MacnhineItem _temp = q.Peek();
                _temp.EndTime = temp.EndTime + _temp.Task[this];
            }

            return task;
        }
        public void Reset()
        {
            q.Clear();
            DowntimeList.Clear();
            _lastTime = 0;
        }
    }
    class Task
    {
        private static int _id = 1;
        public int ID { get; } = _id++;
        //machine - time
        private Dictionary<Machine, int> Times = new Dictionary<Machine, int>();

        public bool RemoveMachine(Machine machine) => Times.Remove(machine);
        public int this[Machine m]
        {
            get
            {
                return Times[m];
            }
            set
            {
                if (!Times.ContainsKey(m))
                    Times.Add(m, 0);
                Times[m] = value;
            }
        }
    }

    class JohnsonTask
    {
        private void _updateResults(List<Solution> bestResults, Solution result)
        {
            if (bestResults.Count == 0)
                bestResults.Add(result);
            else
            {
                int cmp = result.CompareTo(bestResults.First());
                if (cmp == 1)
                    return;
                if (cmp == -1)
                    bestResults.Clear();
                bestResults.Add(result);
            }
        }

        public List<Solution> SolveStupid(List<Task> tasks, List<Machine> machines)
        {
            List<Solution> results = new List<Solution>();
            List<Solution> bestResults = new List<Solution>();

            var permutates = tasks.Permute().Select(ie => ie.ToList()).ToList();
            int id = 0;
            foreach (var perm in permutates)
            {
                id++;
                var res = _solve(perm, machines);
                results.Add(res);

                if (bestResults.Count == 0)
                    bestResults.Add(res);
                else
                {
                    int cmp = res.CompareTo(bestResults.First());
                    if (cmp == 1)
                        continue;
                    if (cmp == -1)
                        bestResults.Clear();
                    bestResults.Add(res);
                }
            }

            //results.Sort();
            return bestResults;
        }
        public List<Solution> SolveHeuristic(List<Task> tasks, List<Machine> machines)
        {
            int m = machines.Count;

            Machine m1 = new Machine();
            Machine m2 = new Machine();

            tasks.ForEach(t => t[m1] = 0);
            tasks.ForEach(t => t[m2] = 0);

            List<Solution> bestResults = new List<Solution>();

            for (int i = 0; i < m - 1; i++)
            {
                tasks.ForEach(t => t[m1] += t[machines[i]]);
                tasks.ForEach(t => t[m2] += t[machines[m - 1 - i]]);

                var sorted = _johnsonSort(tasks, m1, m2);
                Solution res = _solve(sorted, machines);
                _updateResults(bestResults, res);
            }

            tasks.ForEach(t => t.RemoveMachine(m1));
            tasks.ForEach(t => t.RemoveMachine(m2));

            return bestResults;
        }
        public Solution SolveJohnson(List<Task> tasks, Machine m1, Machine m2)
        {
            var a = _johnsonSort(tasks, m1, m2);
            return _solve(a, new List<Machine>() { m1, m2 });
        }

        public Solution _solve(List<Task> tasks, List<Machine> machines)
        {
            machines.ForEach(m => m.Reset());
            int curTime = 0;

            int curTaskWaitStart = 0;
            int LastTaskEnd = -1;

            while (LastTaskEnd != tasks.Count - 1)
            {
                Machine machine = null;

                if (machines.First().IsEmpty && curTaskWaitStart < tasks.Count)
                {
                    machine = machines.First();
                    machine.AddTask(tasks[curTaskWaitStart], curTime);
                    curTaskWaitStart++;
                    continue;
                }

                machines.ForEach(m =>
                {
                    if (!m.IsEmpty && (machine == null || m.EndTime < machine.EndTime))
                        machine = m;
                });

                curTime = machine.EndTime;
                var task = machine.RemoveCurrentTask();
                var index = machines.IndexOf(machine);
                //end
                if (index == machines.Count - 1)
                    LastTaskEnd++;
                else
                    machines[index + 1].AddTask(task, curTime);
            }
            List<TimeInterval> downtimeList = new List<TimeInterval>();
            machines.ForEach(m => downtimeList.AddRange(m.DowntimeList));

            List<int> dl = new List<int>();
            machines.ForEach(m => dl.Add(_computeDowntime(m.DowntimeList)));
            return new Solution() { Tasks = tasks, Downtime = _computeDowntime(downtimeList), AllTime = curTime, DowntimeList = dl };
        }
        private List<Task> _johnsonSort(List<Task> tasks, Machine m1, Machine m2)
        {
            var _tasks = tasks.ToList();
            _tasks.Sort((x, y) => Math.Min(x[m1], x[m2]).CompareTo(Math.Min(y[m1], y[m2])));
            List<Task> a = new List<Task>(), b = new List<Task>();
            for (int i = 0; i < _tasks.Count; ++i)
                (_tasks[i][m1] <= _tasks[i][m2] ? a : b).Add(_tasks[i]);
            b.Reverse();
            a.AddRange(b);

            return a;
        }
        private int _computeDowntime(List<TimeInterval> downtimeList)
        {
            downtimeList.Sort();

            int res = 0;
            int last = 0;
            foreach (var ti in downtimeList)
            {
                res += Math.Max(last, ti.End) - Math.Max(last, ti.Start);
                last = Math.Max(last, ti.End);
            }
            return res;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Machine m1 = new Machine();
            Machine m2 = new Machine();
            List<Task> tasks = new List<Task>();
            List<Machine> machines = new List<Machine>();
            Random r = new Random();
            //for (int i = 0; i < 10; i++)
            //{
            //    Task t = new Task();
            //    t[m1] = r.Next(1, 20);
            //    t[m2] = r.Next(1, 20);
            //    mv.Add(t);
            //    //Console.WriteLine(t[m1] + " " + t[m2]);
            //}
            machines.Add(m1);
            machines.Add(m2);

            var lines = File.ReadAllLines("data.txt");

            int id = 0;
            while (true)
            {
                int m = r.Next(2, 10);
                int n = r.Next(2, 6);
                m = 2;
                tasks = new List<Task>();
                machines = new List<Machine>();

                for (int i = 0; i < m; i++)
                    machines.Add(new Machine());

                for (int i = 0; i < n; i++)
                {
                    Task t = new Task();
                    for (int j = 0; j < m; j++)
                        // t[machines[i]] = int.Parse(split[i]);
                        t[machines[j]] = r.Next(1, 100);
                    tasks.Add(t);
                }
                var res2 = new JohnsonTask().SolveHeuristic(tasks, machines);
                var res3 = new JohnsonTask().SolveStupid(tasks, machines);

                //using (StreamWriter stream = new StreamWriter(File.Create($"{id++}.txt")))
                //{
                //    stream.WriteLine($"{n} {m}");
                //    for (int i = 0; i < n; i++)
                //    {
                //        for (int j = 0; j < m; j++)
                //            stream.Write($"{tasks[i][machines[j]]} ");
                //        stream.WriteLine();
                //    }
                //    stream.WriteLine($"{res2.Count} {res2.First().AllTime} {res2.First().Downtime}");
                //    stream.WriteLine($"{res3.Count} {res3.First().AllTime} {res3.First().Downtime}");
                //}
                

                if (res2.First().AllTime != res3.First().AllTime && res2.First().Downtime != res3.First().Downtime)
                {

                }
            }
            //var res = list.Permute().ToList();
            //var res = list.Permute().Select(ie => ie.ToList()).ToList();
            //Console.WriteLine(res.Count);
            //return;
            //new FlowTask().Solve();
        }
    }
}
