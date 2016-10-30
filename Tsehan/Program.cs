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

    struct TimeInterval
    {
        public int Start { get; set; }
        public int End { get; set; }
        public TimeInterval(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }
    }
    class Solution
    {
        public int AllTime { get; set; }
        public int Downtime { get; set; }
        public List<int> DowntimeList { get; set; }
        public List<List<Task>> Tasks { get; set; }

        public static int Comparator(Solution x, Solution y)
        {
            int cmp = x.AllTime.CompareTo(y.AllTime);
            if (cmp != 0)
                return cmp;
            return x.Downtime.CompareTo(y.Downtime);
        }
    }
    class Machine
    {
        class Temp
        {
            public Task Task { get; set; } = null;
            public int EndTime { get; set; } = -1;
            public int ArrivalTime { get; set; } = -1;
        }
        Queue<Temp> q = new Queue<Temp>();
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
            var temp = new Temp();
            temp.Task = task;
            temp.ArrivalTime = time;

            if (q.Count == 0)
            {
                temp.EndTime = temp.ArrivalTime + task[this];

                if (_lastTime != time)
                    DowntimeList.Add(new TimeInterval(_lastTime, time));
            }

            q.Enqueue(temp);
        }
        public Task RemoveTask()
        {
            Temp temp = q.Dequeue();
            Task task = temp.Task;
            _lastTime = temp.EndTime;

            if (q.Count > 0)
            {
                Temp _temp = q.Peek();
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

    class FlowTask
    {
        public Solution SolveStupid(List<Task> tasks, List<Machine> machines)
        {
            Solution best = null;

            List<Solution> results = new List<Solution>();
            var permutates = tasks.Permute().Select(ie => ie.ToList()).ToList();
            int id = 0;
            foreach (var perm in permutates)
            {
                id++;
                var res = _solve(perm, machines);
                if (best == null)
                    best = res;
                
                if (res.AllTime < best.AllTime && res.DowntimeList.Sum() > best.DowntimeList.Sum())
                {
                    Console.WriteLine();
                }
                results.Add(res);
            }

            results.Sort(Solution.Comparator);
            return results.First();
        }
        public Solution SolveHeuristic(List<Task> tasks, List<Machine> machines)
        {
            int m = machines.Count;

            Machine m1 = new Machine();
            Machine m2 = new Machine();

            tasks.ForEach(t => t[m1] = 0);
            tasks.ForEach(t => t[m2] = 0);

            List<Solution> results = new List<Solution>();
            for (int i = 0; i < m - 1; i++)
            {
                tasks.ForEach(t => t[m1] += t[machines[i]]);
                tasks.ForEach(t => t[m2] += t[machines[m - 1 - i]]);

                var sorted = _johnsonSort(tasks, m1, m2);
                Solution solution = _solve(sorted, machines);
                results.Add(solution);
            }

            tasks.ForEach(t => t.RemoveMachine(m1));
            tasks.ForEach(t => t.RemoveMachine(m2));

            results.Sort(Solution.Comparator);
            results.ForEach(r => Console.WriteLine(r.AllTime + " " + r.Downtime));
            return results.First();
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

                if (machine != null)
                {
                    if (machine.EndTime == -1) throw new Exception("end time == -1");
                    curTime = machine.EndTime;
                    var task = machine.RemoveTask();

                    var index = machines.IndexOf(machine);

                    //end
                    if (index == machines.Count - 1)
                    {
                        LastTaskEnd++;
                    }
                    else
                    {
                        Machine next = machines[index + 1];
                        next.AddTask(task, curTime);
                    }
                }
                else
                {
                    throw new Exception("machine is null");
                }
            }
            List<TimeInterval> downtimeList = new List<TimeInterval>();
            machines.ForEach(m => downtimeList.AddRange(m.DowntimeList));

            List<int> dl = new List<int>();
            machines.ForEach(m => dl.Add(_computeDowntime(m.DowntimeList)));
            return new Solution() { Tasks = new List<List<Task>> { tasks }, Downtime = _computeDowntime(downtimeList), AllTime = curTime, DowntimeList = dl };
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
            downtimeList.Sort((a, b) =>
            {
                int cmp = a.Start.CompareTo(b.Start);
                if (cmp != 0)
                    return cmp;
                return a.End.CompareTo(b.End);
            });

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
            List<Task> mv = new List<Task>();
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
            int m = 5;
            int n = 6;


            mv = new List<Task>();
            machines = new List<Machine>();

            for (int i = 0; i < m; i++)
                machines.Add(new Machine());


            foreach (var line in lines)
            {
                if (n == 0)
                    break;
                n--;

                var split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Task t = new Task();
                for (int i = 0; i < m; i++)
                    // t[machines[i]] = int.Parse(split[i]);
                    t[machines[i]] = r.Next(1, 20);
                mv.Add(t);
            }

            //var res1 = new FlowTask().SolveJonhson(mv, m1, m2);
            //var res2 = new FlowTask().SolveHeuristic(mv, machines);
            var res3 = new FlowTask().SolveStupid(mv, machines);
            //var res4 = new FlowTask()._solve(mv, machines);

            //var res = list.Permute().ToList();
            //var res = list.Permute().Select(ie => ie.ToList()).ToList();
            //Console.WriteLine(res.Count);
            //return;
            //new FlowTask().Solve();
        }
    }
}
