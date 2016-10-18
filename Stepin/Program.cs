using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepin
{

    class AntProblem
    {
        int max_iteration = 1000;

        int n = 10;
        int m = 10;
        List<int> bestWay = new List<int>();
        double bestLength = -1;


        double alpha = 0.5;
        double beta = 0.5;
        double defaultTrail = 1;
        double evaporation = 0.5;
        public double[][] Distances;
        double Q = 100;
        double[][] trails;

        Random random = new Random();
        List<Ant> ants = new List<Ant>();
        class Ant
        {
            AntProblem _parent = null;
            public List<int> Way => _way.ToList();

            public double Length { get; private set; } = -1;
            public int CurTown { get; private set; } = -1;
            public int StartTown { get; private set; } = -1;

            List<int> _way = new List<int>();
            bool[] _visited;
            public Ant(AntProblem parent, int startTown = -1)
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
                    Length = -1;
                    return;
                }
                _visited[town] = true;
                _way.Add(town);
                Length += _parent.Distances[CurTown][town];
            }
            public void GoToNext()
            {
                GoTo(NextTown);
            }
            public void GoToStart()
            {
                GoTo(StartTown);
            }
            public int NextTown
            {
                get
                {
                    int n = _parent.Distances.Length;

                    int i = CurTown;
                    double[] temp = new double[n];
                    double summ = 0;

                    List<int> allowed = new List<int>();
                    for (int j = 0; j < n; j++)
                        if (i != j && _parent.Distances[i][j] != -1 && !_visited[j])
                            allowed.Add(j);

                    foreach (int j in allowed)
                    {
                        temp[j] = Math.Pow(_parent.trails[i][j], _parent.alpha) * Math.Pow(1.0 / _parent.Distances[i][j], _parent.beta);
                        summ += temp[j];
                    }

                    double[] prob = new double[n];
                    foreach (int j in allowed)
                        prob[j] = temp[j] / summ;

                    double value = new Random().NextDouble();
                    double curSumm = 0;
                    foreach (int j in allowed)
                    {
                        curSumm += prob[j];
                        if (curSumm >= value)
                            return j;
                    }

                    return -1;
                    //throw new Exception("No town to move next");
                }
            }
        }


        void fillRandom()
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
        public void solve()
        {
            Distances = CreateArray(n, n);
            trails = CreateArray(n, n, defaultTrail);
            fillRandom();
            for (int t = 0; t < max_iteration; t++)
            {
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
            }
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
                if ((bestLength == -1 && ant.Length != -1) || (bestLength != -1 && ant.Length < bestLength))
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
        static void Main(string[] args)
        {
            new AntProblem().solve();
            Console.WriteLine();
        }

    }
}
