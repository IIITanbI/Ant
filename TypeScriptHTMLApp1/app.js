"use strict";
window.onload = () => {
};
function buttonClick() {
    //for (var i = 0; i < 1; i++) {
    var oReq = new XMLHttpRequest();
    oReq.addEventListener("load", function () {
        let content = this.responseText;
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
function CompareNumbers(a, b) {
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
        let cmp = CompareNumbers(a.Start, b.Start);
        if (cmp != 0)
            return cmp;
        return CompareNumbers(a.End, b.End);
    }
}
class Solution {
    CompareTo(b) {
        return this.Compare(this, b);
    }
    Compare(a, b) {
        let cmp = CompareNumbers(a.AllTime, b.AllTime);
        if (cmp != 0)
            return cmp;
        return CompareNumbers(a.Downtime, b.Downtime);
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
        let bestResults = [];
        let permutates = Permutations(tasks);
        permutates.forEach((perm, ind, array) => {
            var res = this._solve(perm, machines);
            this._updateResults(bestResults, res);
        });
        return bestResults;
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
    _johnsonSort(tasks, m1, m2) {
        var _tasks = tasks.slice();
        _tasks.sort((x, y) => CompareNumbers(Math.min(x.getTime(m1), x.getTime(m2)), (Math.min(y.getTime(m1), y.getTime(m2)))));
        let a = [], b = [];
        for (var i = 0; i < _tasks.length; i++)
            (_tasks[i].getTime(m1) <= _tasks[i].getTime(m2) ? a : b).push(_tasks[i]);
        b.reverse();
        let generate = (list, machine) => {
            let results = [[]];
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
                    let temp = [[]];
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
        var res = [[]];
        for (var one of _a) {
            for (var two of _b) {
                var copy = one.concat(two);
                res.push(copy);
            }
        }
        return res;
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
    _updateResults(bestResults, result) {
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
//# sourceMappingURL=app.js.map