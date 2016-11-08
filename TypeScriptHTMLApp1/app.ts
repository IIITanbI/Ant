import * as Collections from "typescript-collections";



window.onload = () => {
};
function buttonClick() {
    //for (var i = 0; i < 1; i++) {
    var oReq = new XMLHttpRequest();
    oReq.addEventListener("load", function () {
        let content = (this as XMLHttpRequest).responseText;

        var arrayOfLines = content.match(/[^\r\n]+/g);
        for (var match of arrayOfLines) {
            console.log(match);
        }
        console.log("load");
    });
    oReq.addEventListener("error", function () {
        console.log("error");
    });
    oReq.addEventListener("abort", function () {
        console.log("abort");
    });
    let file = 0 + ".txt";
    oReq.open("GET", "http://yourdomain.com:8887/" + file);
    oReq.send();
    //}
}




interface IComparable<T> {
    CompareTo(obj: T): number;
}
interface ICompararer<T> {
    Compare(obj1: T, obj2: T): number;
}
function CompareNumbers(a: number, b: number): number {
    if (a == b)
        return 0;
    return a < b ? -1 : 1;
}
function Permutations<T>(list: T[]): T[][] {
    if (list.length == 0)
        return [[]];

    var result = [];

    for (var i = 0; i < list.length; i++) {
        // Clone list (kind of)
        var copy = Object.create(list);

        // Cut one element from list
        var head = copy.splice(i, 1);

        // Permute rest of list
        var rest = Permutations(copy);

        // Add head to each permutation of rest of list
        for (var j = 0; j < rest.length; j++) {
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

    constructor(start: number, end: number) {
        this.Start = start;
        this.End = end;
    }

    public CompareTo = (b: TimeInterval): number => this.Compare(this, b);
    public Compare(a: TimeInterval, b: TimeInterval): number {
        let cmp = CompareNumbers(a.Start, b.Start);
        if (cmp != 0)
            return cmp;
        return CompareNumbers(a.End, b.End);
    }
}
class Solution implements IComparable<Solution>, ICompararer<Solution>
{
    public AllTime: number;
    public Downtime: number;
    public DowntimeList: number[];
    public Tasks: Task[];

    public CompareTo(b: Solution): number {
        return this.Compare(this, b);
    }
    public Compare(a: Solution, b: Solution): number {
        let cmp = CompareNumbers(a.AllTime, b.AllTime);
        if (cmp != 0)
            return cmp;
        return CompareNumbers(a.Downtime, b.Downtime);
    }
}
class MacnhineItem {
    public Task: Task;
    public EndTime: number = -1;
    public ArrivalTime: number = -1;
}
class Machine {
    q: Collections.Queue<MacnhineItem>;
    public DowntimeList: TimeInterval[];

    public get IsEmpty(): boolean { return this.q.isEmpty(); }
    public get EndTime(): number { return !this.q.isEmpty() ? this.q.peek().EndTime : -1; }
    private _lastTime: number;

    constructor() {
        this.Reset();
    }
    public AddTask(task: Task, time: number) {
        var temp = new MacnhineItem();
        temp.Task = task;
        temp.ArrivalTime = time;

        if (this.IsEmpty) {
            temp.EndTime = temp.ArrivalTime + task.getTime(this);

            if (this._lastTime != time)
                this.DowntimeList.push(new TimeInterval(this._lastTime, time));
        }

        this.q.enqueue(temp);
    }
    public RemoveCurrentTask(): Task {
        var temp = this.q.dequeue();
        var task = temp.Task;
        this._lastTime = temp.EndTime;

        if (!this.IsEmpty) {
            var _temp = this.q.peek();
            _temp.EndTime = temp.EndTime + _temp.Task.getTime(this);
        }

        return task;
    }
    public Reset() {
        this.q.clear();
        this.DowntimeList = [];
        this._lastTime = 0;

    }
}
class Task {
    private static _id: number = 1;
    public get ID(): number { return Task._id++; }

    private Times: Collections.FactoryDictionary<Machine, number>;

    public getTime(machine: Machine): number { return this.Times.getValue(machine); }
    public setTime(machine: Machine, value: number) { this.Times.setValue(machine, value); }

    public RemoveMachine(machine: Machine): boolean { return this.Times.remove(machine) !== undefined; }
}

class JohnsonTask {
    public SolveStupid(tasks: Task[], machines: Machine[]): Solution[] {
        let bestResults: Solution[] = [];
        let permutates = Permutations(tasks);

        permutates.forEach((perm, ind, array) => {
            var res = this._solve(perm, machines);
            this._updateResults(bestResults, res);
        });

        return bestResults;
    }
    public SolveHeuristic(tasks: Task[], machines: Machine[]): Solution[] {
        let m = machines.length;

        let m1: Machine = new Machine();
        let m2: Machine = new Machine();

        tasks.forEach(t => t.setTime(m1, 0));
        tasks.forEach(t => t.setTime(m2, 0));

        let results: Solution[] = [];
        let bestResults: Solution[] = [];

        for (var i = 0; i < m - 1; i++) {
            tasks.forEach(t => t.setTime(m1, t.getTime(m1) + t.getTime(machines[i])));
            tasks.forEach(t => t.setTime(m2, t.getTime(m2) + t.getTime(machines[m - 1 - i])));

            var sortedCombintaions = this._johnsonSort(tasks, m1, m2);
            var temp = this._solve(sortedCombintaions[0], machines);
            for (var combintation of sortedCombintaions) {
                var res = this._solve(combintation, machines);
                if (temp.CompareTo(res) != 0) {
                    console.warn("warning");
                }
                this._updateResults(bestResults, res);
            }
            this._updateResults(bestResults, res);
        }

        tasks.forEach(t => t.RemoveMachine(m1));
        tasks.forEach(t => t.RemoveMachine(m2));

        return bestResults;
    }

    private _solve(tasks: Task[], machines: Machine[]): Solution {
        machines.forEach(m => m.Reset());
        let curTime = 0;

        let curTaskWaitStart = 0;
        let LastTaskEnd = -1;

        while (LastTaskEnd != tasks.length - 1) {
            let machine: Machine = null;

            if (machines[0].IsEmpty && curTaskWaitStart < tasks.length) {
                machine = machines[0];
                machine.AddTask(tasks[curTaskWaitStart], curTime);
                curTaskWaitStart++;
                continue;
            }

            machines.forEach(m => {
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
    private _johnsonSort(tasks: Task[], m1: Machine, m2: Machine): Task[][] {
        var _tasks = tasks.slice();
        _tasks.sort((x, y) => CompareNumbers(Math.min(x.getTime(m1), x.getTime(m2)), (Math.min(y.getTime(m1), y.getTime(m2)))));
        let a: Task[] = [], b: Task[] = [];
        for (var i = 0; i < _tasks.length; i++)
            (_tasks[i].getTime(m1) <= _tasks[i].getTime(m2) ? a : b).push(_tasks[i]);
        b.reverse();

        let generate = (list: Task[], machine: Machine): Task[][] => {
            let results: Task[][] = [[]];
            var _originalCopy = list.slice();
            results.push(_originalCopy);

            if (_originalCopy.length == 0)
                return results;

            var current = _originalCopy[0].getTime(machine);
            var count = 1;
            for (var i = 1; i <= _originalCopy.length; i++) {
                if (i == _originalCopy.length || _originalCopy[i].getTime(machine) != current) {
                    var from = i - count;
                    var range = _originalCopy.slice(from, from + count);
                    var permutates = Permutations(range);

                    let temp: Task[][] = [[]];

                    for (var original of results) {
                        for (var permute of permutates) {
                            var copy = original.slice();
                            copy.splice(from, count, ...permute);
                            temp.push(copy);
                        }
                    }

                    results = [[]];
                    results = temp;

                    if (i == _originalCopy.length)
                        break;

                    current = _originalCopy[i].getTime(machine);
                    count = 1;
                }
                else
                    count++;
            }

            return results;
        };
        // a = a.concat(b);

        var _a = generate(a, m1);
        var _b = generate(b, m2);

        var res: Task[][] = [[]];
        for (var one of _a) {
            for (var two of _b) {
                var copy = one.concat(two);
                res.push(copy);
            }
        }

        return res;
    }
    private _computeDowntime(downtimeList: TimeInterval[]): number {
        downtimeList.sort(TimeInterval.prototype.Compare);

        let res: number = 0;
        let last: number = 0;
        downtimeList.forEach(ti => {
            res += Math.max(last, ti.End) - Math.max(last, ti.Start);
            last = Math.max(last, ti.End);
        });
        return res;
    }
    private _updateResults(bestResults: Solution[], result: Solution) {
        if (bestResults.length == 0)
            bestResults.push(result);
        else {
            var cmp = result.CompareTo(bestResults[0]);
            if (cmp == 1)
                return;
            if (cmp == -1)
                bestResults = [];
            bestResults.push(result);
        }
    }
}

