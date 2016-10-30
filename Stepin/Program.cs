using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepin
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
    class AntProblem
    {
        int max_iteration = 10000;

        int n = 0;
        int m = 0;
        List<int> bestWay = new List<int>();
        double bestLength = -1;


        double alpha = 0.6;
        double beta = 0.3;
        double defaultTrail = 1;
        double evaporation = 0.5;
        public double[][] Distances;
        double Q = 300;
        double[][] trails;

        Random random = new Random();
        List<Ant> ants = new List<Ant>();
        double[][] nu = null;

        public void Refresh()
        {
            int n = Distances.Length;
            nu = new double[n][];
            for (int i = 0; i < n; i++)
                nu[i] = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    nu[i][j] = Math.Pow(trails[i][j], alpha) * Math.Pow(1.0 / Distances[i][j], beta);
                }
            }
        }
        class Ant
        {
            AntProblem _parent = null;
            public List<int> Way => _way;

            public bool IsAlive { get; set; } = true;
            public double Length { get; private set; } = 0;
            public int CurTown { get; private set; } = -1;
            public int StartTown { get; private set; } = -1;

            List<int> _way = new List<int>();
            bool[] _visited;



            public Ant(AntProblem parent, int startTown)
            {
                _parent = parent;
                CurTown = startTown;
                StartTown = startTown;
                _visited = new bool[_parent.Distances.Length];
                _visited[StartTown] = true;
                _way.Add(StartTown);
            }

            private void GoTo(int town)
            {
                if (town == -1 || _parent.Distances[CurTown][town] == -1)
                {
                    IsAlive = false;
                    return;
                }
                _visited[town] = true;
                _way.Add(town);
                Length += _parent.Distances[CurTown][town];
                CurTown = town;
            }
            public void GoToNext()
            {
                GoTo(NextTown());
            }
            public void GoToStart()
            {
                GoTo(StartTown);
            }

            public int NextTown()
            {
                int n = _parent.Distances.Length;

                int i = CurTown;
                double summ = 0;

                List<int> allowed = new List<int>();
                for (int j = 0; j < n; j++)
                    if (i != j && _parent.Distances[i][j] != -1 && !_visited[j])
                        allowed.Add(j);

                foreach (int j in allowed)
                {
                    summ += _parent.nu[i][j];
                }

                double[] prob = new double[n];
                foreach (int j in allowed)
                    prob[j] = _parent.nu[i][j] / summ;

                double value = new Random().NextDouble();
                double curSumm = 0;
                foreach (int j in allowed)
                {
                    curSumm += prob[j];
                    if (curSumm >= value)
                        return j;
                }

                return -1;
            }
        }


        void fillRandomDistances()
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        Distances[i][j] = 0;
                        continue;
                    }
                    int t = random.Next();
                    if (t % 2 == 0)
                    {
                        Distances[i][j] = -1;
                    }
                    else
                    {
                        int d = random.Next(1000);
                        Distances[i][j] = d;
                    }
                }
            }
        }
        public List<int> solve()
        {
            //Distances = CreateArray(n, n);
            //fillRandomDistances();
            n = Distances.Length;
            m = n;
            trails = CreateArray(n, n, defaultTrail);


            for (int t = 0; t < max_iteration; t++)
            {
                Refresh();
                ants.Clear();
                for (int k = 0; k < m; k++)
                    ants.Add(new Ant(this, random.Next(0, n - 1)));

                for (int k = 0; k < m; k++)
                {
                    for (int town = 0; town < n - 1; town++)
                        ants[k].GoToNext();
                    ants[k].GoToStart();
                }
                UpdateTrails();
                UpdateBest();
            }
            return bestWay;
        }

        void UpdateTrails()
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    trails[i][j] *= evaporation;

            foreach (Ant ant in ants)
            {
                var way = ant.Way;
                double dPh = Q / ant.Length;
                //1 -> 2 -> 3 -> 4 -> 1
                for (int i = 0; i < way.Count - 1; i++)
                    trails[way[i]][way[i + 1]] += dPh;
            }
        }
        void UpdateBest()
        {
            foreach (Ant ant in ants)
            {
                if (ant.IsAlive && (bestLength == -1 || ant.Length < bestLength))
                {
                    bestLength = ant.Length;
                    bestWay = ant.Way;
                }
            }
        }
        static double[][] CreateArray(int n, int m, double defaultValue = 0)
        {
            var array = new double[n][];
            for (int i = 0; i < n; i++)
                array[i] = new double[m];

            if (defaultValue != 0)
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < m; j++)
                        array[i][j] = defaultValue;

            return array;
        }
    }


    class Program
    {
        static void PrintMatrix(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix.Length; j++)
                {
                    var value = Math.Round(matrix[i][j], 2);
                    Console.Write(value.ToString().PadLeft(6));
                }
                Console.WriteLine();
            }
        }
        static void PrintMatrixInFile(double[][] matrix, string file)
        {
            StringBuilder build = new StringBuilder();
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix.Length; j++)
                {
                    var value = Math.Round(matrix[i][j], 2);
                    build.Append(value.ToString().PadLeft(6));
                }
                build.AppendLine();
            }
            string str = build.ToString();
            File.WriteAllText(file, str);
        }
        public static double[][] RandomDst(int n)
        {
            Random random = new Random();
            double[][] distances = new double[n][];
            for (int i = 0; i < n; i++)
                distances[i] = new double[n];

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    var dst = random.Next(10, 20);
                    distances[i][j] = dst;
                    distances[j][i] = dst;
                }
            }
            return distances;
        }
        static void Main(string[] args)
        {
            //string folder = @"C:\Users\1\Downloads\ALL_tsp";
            //string file = "a280";
            //var distances = ParseDistances(Path.Combine(folder, file + ".tsp"));
            var distances = RandomDst(8);
            // var trueSolve = ParseSolve(Path.Combine(folder, file + ".opt.tour"));
            var trueSolve = StupidSolve(distances);
            //PrintMatrix(distances);
            PrintMatrixInFile(distances, "file.txt");
            Console.WriteLine("Start");

            Stopwatch sw = Stopwatch.StartNew();
            var antProblem = new AntProblem();
            antProblem.Distances = distances;
            var antSolve = antProblem.solve();
            sw.Stop();
            Console.WriteLine("Elapsed: "+ (double)sw.ElapsedMilliseconds/1000.0);

            double res1 = 0;
            for (int i = 0; i < antSolve.Count - 1; i++) res1 += distances[antSolve[i]][antSolve[i + 1]];

            double res2 = 0;
            for (int i = 0; i < trueSolve.Count - 1; i++) res2 += distances[trueSolve[i]][trueSolve[i + 1]];
            Console.WriteLine(res1);
            Console.WriteLine(res2);
            Console.WriteLine();
        }

        static List<int> StupidSolve(double[][] distances)
        {
            int n = distances.Length;
            List<int> way = new List<int>();
            for (int i = 1; i < n; i++)
                way.Add(i);

            var permutes = way.Permute().Select(ie => ie.ToList()).ToList();

            double bestLength = double.MaxValue;
            List<int> bestWay = new List<int>();
            foreach (var permute in permutes)
            {
                double temp = 0;
                temp += distances[0][permute[0]];
                for (int i = 0; i < permute.Count - 1; i++)
                    temp += distances[permute[i]][permute[i + 1]];
                temp += distances[permute.Last()][0];

                if (temp < bestLength)
                {
                    bestLength = temp;
                    bestWay = permute;
                }
            }
            List<int> res = new List<int>();
            res.Add(0);
            res.AddRange(bestWay);
            res.Add(0);
            return res;
        }
        static double[][] ParseDistances(string file)
        {
            var allLines = File.ReadAllLines(file);
            var lines = allLines.SkipWhile(s => s != "NODE_COORD_SECTION").Skip(1).TakeWhile(s => s != "EOF").ToList();

            double[][] res = new double[lines.Count][];
            List<Tuple<double, double>> points = new List<Tuple<double, double>>();
            for (int i = 0; i < lines.Count; i++)
            {
                var split = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                points.Add(new Tuple<double, double>(double.Parse(split[1]), double.Parse(split[2])));
            }
            for (int i = 0; i < points.Count; i++)
            {
                res[i] = new double[points.Count];
                for (int j = 0; j < points.Count; j++)
                {
                    res[i][j] = Math.Sqrt(Math.Pow(points[i].Item1 - points[j].Item1, 2) + Math.Pow(points[i].Item2 - points[j].Item2, 2));
                }
            }
            return res;
        }
        static List<int> ParseSolve(string file)
        {
            var allLines = File.ReadAllLines(file);
            var lines = allLines.SkipWhile(s => s != "TOUR_SECTION").Skip(1).TakeWhile(s => s != "-1").ToList();

            return lines.Select(l => int.Parse(l)).ToList();
        }
    }
}
