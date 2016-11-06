import * as Collections from "typescript-collections";

class Greeter
{
    element: HTMLElement;
    span: HTMLElement;
    timerToken: number;

    constructor(element: HTMLElement)
    {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement("span");
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }

    start()
    {
        this.timerToken = setInterval(() => this.span.innerHTML = new Date().toUTCString(), 500);
    }

    stop()
    {
        clearTimeout(this.timerToken);
    }

}

window.onload = () =>
{

    let el = document.getElementById("content");
    let greeter = new Greeter(el);
    // greeter.start();
};




interface IComparable<T>
{
    CompareTo(obj: T): number;
}
interface ICompararer<T>
{
    Compare(obj1: T, obj2: T): number;
}
function CompareTo(a: number, b: number): number
{
    if (a == b)
        return 0;
    return a < b ? -1 : 1;
}
function Permutations<T>(list: T[]): T[][]
{
    if (list.length == 0)
        return [[]];

    var result = [];

    for (var i = 0; i < list.length; i++)
    {
        // Clone list (kind of)
        var copy = Object.create(list);

        // Cut one element from list
        var head = copy.splice(i, 1);

        // Permute rest of list
        var rest = Permutations(copy);

        // Add head to each permutation of rest of list
        for (var j = 0; j < rest.length; j++)
        {
            var next = head.concat(rest[j]);
            result.push(next);
        }
    }

    return result;
}

class TimeInterval implements IComparable<TimeInterval>, ICompararer<TimeInterval>
{
    public Start: number;
    public End: number;

    constructor(start: number, end: number)
    {
        this.Start = start;
        this.End = end;
    }


    public CompareTo = (b: TimeInterval): number => this.Compare(this, b);

    public Compare(a: TimeInterval, b: TimeInterval): number
    {
        let cmp = CompareTo(a.Start, b.Start);
        if (cmp != 0)
            return cmp;
        return CompareTo(a.End, b.End);
    }
}
class Solution implements IComparable<Solution>
{
    public AllTime: number;
    public Downtime: number;
    public DowntimeList: number[];
    public Tasks: Task[];


    public CompareTo = (b: Solution): number => this.Compare(this, b);
    public Compare(a: Solution, b: Solution): number 
    {
        let cmp = CompareTo(a.AllTime, b.AllTime);
        if (cmp != 0)
            return cmp;
        return CompareTo(a.Downtime, b.Downtime);
    }
}
class MacnhineItem
{
    public Task: Task;
    public EndTime: number = -1;
    public ArrivalTime: number = -1;
}
class Machine
{
    q: Collections.Queue<MacnhineItem>;
    public DowntimeList: TimeInterval[];

    public get IsEmpty(): boolean { return this.q.isEmpty(); }
    public get EndTime(): number { return !this.q.isEmpty() ? this.q.peek().EndTime : -1; }
    private _lastTime: number;

    constructor()
    {
        this.Reset();
    }
    public AddTask(task: Task, time: number)
    {
        var temp = new MacnhineItem();
        temp.Task = task;
        temp.ArrivalTime = time;

        if (this.IsEmpty)
        {
            temp.EndTime = temp.ArrivalTime + task.getTime(this);

            if (this._lastTime != time)
                this.DowntimeList.push(new TimeInterval(this._lastTime, time));
        }

        this.q.enqueue(temp);
    }
    public RemoveCurrentTask(): Task
    {
        var temp = this.q.dequeue();
        var task = temp.Task;
        this._lastTime = temp.EndTime;

        if (!this.IsEmpty)
        {
            var _temp = this.q.peek();
            _temp.EndTime = temp.EndTime + _temp.Task.getTime(this);
        }

        return task;
    }
    public Reset()
    {
        this.q.clear();
        this.DowntimeList = [];
        this._lastTime = 0;

    }
}
class Task
{
    private static _id: number = 1;
    public get ID(): number { return Task._id++; }

    private Times: Collections.FactoryDictionary<Machine, number>;

    public getTime(machine: Machine): number { return this.Times.getValue(machine); }
    public setTime(machine: Machine, value: number) { this.Times.setValue(machine, value); }

    public RemoveMachine(machine: Machine): boolean { return this.Times.remove(machine) !== undefined; }
}

class JohnsonTask
{
    public SolveStupid(tasks: Task[], machines: Machine[]): Solution
    {
        let best: Solution = null;
        let results: Solution[] = [];
        let permutates = Permutations(tasks);
        let id: number = 0;
        permutates.forEach((perm, ind, array) =>
        {
            id++;
            var res = this._solve(perm, machines);
            var res = new Solution();
            if (best == null)
                best = res;

            if (res.AllTime < best.AllTime && res.DowntimeList.reduce((pv, cv, ci, ar) => pv + cv, 0) > best.DowntimeList.reduce((pv, cv, ci, ar) => pv + cv, 0))
            {
                console.log();
            }
            results.push(res);
        });



        results.sort(Solution.prototype.CompareTo);
        return results[0];
    }
    public SolveHeuristic(tasks: Task[], machines: Machine[]): Solution[]
    {
        let m = machines.length;

        let m1: Machine = new Machine();
        let m2: Machine = new Machine();

        tasks.forEach(t => t.setTime(m1, 0));
        tasks.forEach(t => t.setTime(m2, 0));

        let results: Solution[] = [];
        let bestResults: Solution[] = [];

        for (var i = 0; i < m - 1; i++)
        {
            tasks.forEach(t => t.setTime(m1, t.getTime(m1) + t.getTime(machines[i])));
            tasks.forEach(t => t.setTime(m2, t.getTime(m2) + t.getTime(machines[m - 1 - i])));

            let sorted = this._johnsonSort(tasks, m1, m2);
            let res: Solution = this._solve(sorted, machines);
            results.push(res);

            if (bestResults.length == 0)
                bestResults.push(res);
            else
            {
                let cmp = res.CompareTo(bestResults[0]);
                if (cmp == -1)
                    continue;
                if (cmp == 1)
                    bestResults = [];
                bestResults.push(res);
            }
        }

        tasks.forEach(t => t.RemoveMachine(m1));
        tasks.forEach(t => t.RemoveMachine(m2));

        results.sort(Solution.prototype.Compare);
        results.forEach(r => console.log(r.AllTime + " " + r.Downtime));
        return bestResults;
    }

    public _solve(tasks: Task[], machines: Machine[]): Solution
    {
        machines.forEach(m => m.Reset());
        let curTime = 0;

        let curTaskWaitStart = 0;
        let LastTaskEnd = -1;

        while (LastTaskEnd != tasks.length - 1)
        {
            let machine: Machine = null;

            if (machines[0].IsEmpty && curTaskWaitStart < tasks.length)
            {
                machine = machines[0];
                machine.AddTask(tasks[curTaskWaitStart], curTime);
                curTaskWaitStart++;
                continue;
            }

            machines.forEach(m =>
            {
                if (!m.IsEmpty && (machine == null || m.EndTime < machine.EndTime))
                    machine = m;
            });

            curTime = machine.EndTime;
            var task = machine.RemoveCurrentTask();
            var index = machines.indexOf(machine);
            //end
            if (index == machines.length - 1)
                LastTaskEnd++;
            else
                machines[index + 1].AddTask(task, curTime);
        }
        let downtimeList: TimeInterval[] = [];
        machines.forEach(m => downtimeList = downtimeList.concat(m.DowntimeList));

        let dl: number[] = [];
        machines.forEach(m => dl.push(this._computeDowntime(m.DowntimeList)));

        let cs: Solution = new Solution();
        cs.Tasks = tasks;
        cs.Downtime = this._computeDowntime(downtimeList);
        cs.AllTime = curTime;
        cs.DowntimeList = dl
        return cs;
        //return new Solution() { Tasks = tasks, Downtime = _computeDowntime(downtimeList), AllTime = curTime, DowntimeList = dl };
    }
    public SolveJohnson(tasks: Task[], m1: Machine, m2: Machine): Solution
    {
        var a = this._johnsonSort(tasks, m1, m2);
        return this._solve(a, [m1, m2]);
    }

    private _johnsonSort(tasks: Task[], m1: Machine, m2: Machine): Task[]
    {
        var _tasks = tasks.slice();
        _tasks.sort((x, y) => CompareTo(Math.min(x.getTime(m1), x.getTime(m2)), (Math.min(y.getTime(m1), y.getTime(m2)))));
        let a: Task[] = [], b: Task[] = [];
        for (var i = 0; i < _tasks.length; i++)
            (_tasks[i].getTime(m1) <= _tasks[i].getTime(m2) ? a : b).push(_tasks[i]);
        b.reverse();
        a = a.concat(b);

        return a;
    }
    private _computeDowntime(downtimeList: TimeInterval[]): number
    {
        downtimeList.sort(TimeInterval.prototype.Compare);

        let res: number = 0;
        let last: number = 0;
        downtimeList.forEach(ti =>
        {
            res += Math.max(last, ti.End) - Math.max(last, ti.Start);
            last = Math.max(last, ti.End);
        });
        return res;
    }
}

