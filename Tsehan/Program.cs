using System;
using System.Collections.Generic;
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
    class Machine
    {
        class Temp
        {
            public Task Task { get; set; } = null;
            public int EndTime { get; set; } = -1;
        }
        Queue<Temp> q = new Queue<Temp>();

        public bool IsEmpty => q.Count == 0;
        public int EndTime => q.Count > 0 ? q.Peek().EndTime : -1;
        public void AddTask(Task task, int time)
        {
            if (q.Count > 0)
                q.Enqueue(new Temp() { Task = task });
            else
                q.Enqueue(new Temp() { Task = task, EndTime = time + task.Times[this] });
        }

        public Task RemoveTask()
        {
            Temp temp = q.Dequeue();
            Task task = temp.Task;

            if (q.Count > 0)
            {
                Temp _temp = q.Peek();
                _temp.EndTime = temp.EndTime + _temp.Task.Times[this];
            }
            return task;
        }

        public void Reset()
        {
            q.Clear();
        }
    }
    class Task
    {
        //machine - time
        public Dictionary<Machine, int> Times = new Dictionary<Machine, int>();

        public void SetTime(Machine machine, int time)
        {
            if (Times.ContainsKey(machine))
                Times[machine] = time;
            else Times.Add(machine, time);
        }
    }

    class FlowTask
    {
        private static Random random = new Random(unchecked(Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId));



        void Init(int machineNumber, int taskNumber, List<Machine> machines, List<Task> tasks)
        {
            int m = machineNumber;
            int n = taskNumber;

            for (int i = 0; i < m; i++)
            {
                machines.Add(new Machine());
            }
            for (int i = 0; i < n; i++)
            {
                var task = new Task();
                tasks.Add(task);
                machines.ForEach(machine => task.SetTime(machine, random.Next(1, 10)));
            }
        }
        void Init1(List<Machine> machines, List<Task> tasks)
        {
            int m = 2;
    

            for (int i = 0; i < m; i++)
            {
                machines.Add(new Machine());
            }

            Task task1 = new Task();
            task1.SetTime(machines[0], 1);
            task1.SetTime(machines[1], 10);

            Task task2 = new Task();
            task2.SetTime(machines[0], 3);
            task2.SetTime(machines[1], 5);
            tasks.Add(task2);
            tasks.Add(task1);

            //for (int i = 0; i < n; i++)
            //{
            //    var task = new Task();
            //    tasks.Add(task);
            //    machines.ForEach(machine => task.SetTime(machine, random.Next(10, 100)));
            //}
        }

        public void Solve()
        {
            List<Machine> machines = new List<Machine>();
            List<Task> tasks = new List<Task>();
            Init(3, 9, machines, tasks);


            List<int> results = new List<int>();
            var permutates = tasks.Permute().Select(ie => ie.ToList()).ToList();
            return;
            foreach (var perm in permutates)
            {
                machines.ForEach(m => m.Reset());
                var res = _solve(machines, perm);
                //Console.WriteLine(res);
                results.Add(res);
            }
            results.Sort();
            results.ForEach(r => Console.WriteLine(r));
        }
        int _solve(List<Machine> machines, List<Task> tasks)
        {
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
            return curTime;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < 10; i++)
                list.Add(i);

            //var res = list.Permute().ToList();
            //var res = list.Permute().Select(ie => ie.ToList()).ToList();
            //Console.WriteLine(res.Count);
            //return;
            new FlowTask().Solve();
        }
    }
}
