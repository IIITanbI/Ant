"use strict";
class Greeter {
    constructor(element) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement("span");
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }
    start() {
        this.timerToken = setInterval(() => this.span.innerHTML = new Date().toUTCString(), 500);
    }
    stop() {
        clearTimeout(this.timerToken);
    }
}
window.onload = () => {
    let el = document.getElementById("content");
    let greeter = new Greeter(el);
    // greeter.start();
};
function CompareTo(a, b) {
    if (a == b)
        return 0;
    return a < b ? -1 : 1;
}
function Permutations(list) {
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
class TimeInterval {
    constructor(start, end) {
        this.CompareTo = (b) => this.Compare(this, b);
        this.Start = start;
        this.End = end;
    }
    Compare(a, b) {
        let cmp = CompareTo(a.Start, b.Start);
        if (cmp != 0)
            return cmp;
        return CompareTo(a.End, b.End);
    }
}
class Solution {
    constructor() {
        this.CompareTo = (b) => this.Compare(this, b);
    }
    Compare(a, b) {
        let cmp = CompareTo(a.AllTime, b.AllTime);
        if (cmp != 0)
            return cmp;
        return CompareTo(a.Downtime, b.Downtime);
    }
}
class MacnhineItem {
    constructor() {
        this.EndTime = -1;
        this.ArrivalTime = -1;
    }
}
class Machine {
    constructor() {
        this.Reset();
    }
    get IsEmpty() { return this.q.isEmpty(); }
    get EndTime() { return !this.q.isEmpty() ? this.q.peek().EndTime : -1; }
    AddTask(task, time) {
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
    RemoveCurrentTask() {
        var temp = this.q.dequeue();
        var task = temp.Task;
        this._lastTime = temp.EndTime;
        if (!this.IsEmpty) {
            var _temp = this.q.peek();
            _temp.EndTime = temp.EndTime + _temp.Task.getTime(this);
        }
        return task;
    }
    Reset() {
        this.q.clear();
        this.DowntimeList = [];
        this._lastTime = 0;
    }
}
class Task {
    get ID() { return Task._id++; }
    getTime(machine) { return this.Times.getValue(machine); }
    setTime(machine, value) { this.Times.setValue(machine, value); }
    RemoveMachine(machine) { return this.Times.remove(machine) !== undefined; }
}
Task._id = 1;
class JohnsonTask {
    SolveStupid(tasks, machines) {
        let best = null;
        let results = [];
        let permutates = Permutations(tasks);
        let id = 0;
        permutates.forEach((perm, ind, array) => {
            id++;
            var res = this._solve(perm, machines);
            var res = new Solution();
            if (best == null)
                best = res;
            if (res.AllTime < best.AllTime && res.DowntimeList.reduce((pv, cv, ci, ar) => pv + cv, 0) > best.DowntimeList.reduce((pv, cv, ci, ar) => pv + cv, 0)) {
                console.log();
            }
            results.push(res);
        });
        results.sort(Solution.prototype.CompareTo);
        return results[0];
    }
    SolveHeuristic(tasks, machines) {
        let m = machines.length;
        let m1 = new Machine();
        let m2 = new Machine();
        tasks.forEach(t => t.setTime(m1, 0));
        tasks.forEach(t => t.setTime(m2, 0));
        let results = [];
        let bestResults = [];
        for (var i = 0; i < m - 1; i++) {
            tasks.forEach(t => t.setTime(m1, t.getTime(m1) + t.getTime(machines[i])));
            tasks.forEach(t => t.setTime(m2, t.getTime(m2) + t.getTime(machines[m - 1 - i])));
            let sorted = this._johnsonSort(tasks, m1, m2);
            let res = this._solve(sorted, machines);
            results.push(res);
            if (bestResults.length == 0)
                bestResults.push(res);
            else {
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
    _solve(tasks, machines) {
        machines.forEach(m => m.Reset());
        let curTime = 0;
        let curTaskWaitStart = 0;
        let LastTaskEnd = -1;
        while (LastTaskEnd != tasks.length - 1) {
            let machine = null;
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
        let downtimeList = [];
        machines.forEach(m => downtimeList = downtimeList.concat(m.DowntimeList));
        let dl = [];
        machines.forEach(m => dl.push(this._computeDowntime(m.DowntimeList)));
        let cs = new Solution();
        cs.Tasks = tasks;
        cs.Downtime = this._computeDowntime(downtimeList);
        cs.AllTime = curTime;
        cs.DowntimeList = dl;
        return cs;
        //return new Solution() { Tasks = tasks, Downtime = _computeDowntime(downtimeList), AllTime = curTime, DowntimeList = dl };
    }
    SolveJohnson(tasks, m1, m2) {
        var a = this._johnsonSort(tasks, m1, m2);
        return this._solve(a, [m1, m2]);
    }
    _johnsonSort(tasks, m1, m2) {
        var _tasks = tasks.slice();
        _tasks.sort((x, y) => CompareTo(Math.min(x.getTime(m1), x.getTime(m2)), (Math.min(y.getTime(m1), y.getTime(m2)))));
        let a = [], b = [];
        for (var i = 0; i < _tasks.length; i++)
            (_tasks[i].getTime(m1) <= _tasks[i].getTime(m2) ? a : b).push(_tasks[i]);
        b.reverse();
        a = a.concat(b);
        return a;
    }
    _computeDowntime(downtimeList) {
        downtimeList.sort(TimeInterval.prototype.Compare);
        let res = 0;
        let last = 0;
        downtimeList.forEach(ti => {
            res += Math.max(last, ti.End) - Math.max(last, ti.Start);
            last = Math.max(last, ti.End);
        });
        return res;
    }
}
//# sourceMappingURL=app.js.map