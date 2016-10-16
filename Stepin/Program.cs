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
        double bestLength = double.MaxValue;


        double alpha, beta, e, Q, defaultTrail = 1;
        public double[][] Distances;
        double[][] trails;

        Random random = new Random();
        List<Ant> ants = new List<Ant>();
        class Ant
        {
            AntProblem _parent = null;
            public List<int> Way => _way.ToList();

            public double Length { get; private set; } = -1;
            public int CurTown { get; private set; } = -1;

            List<int> _way = new List<int>();
            bool[] _visited;
            public Ant(AntProblem parent, int startTown = -1)
            {
                _parent = parent;
                CurTown = startTown;
                _visited = new bool[_parent.Distances.Length];
            }


            public void GoTo(int townNumber)
            {
                _way.Add(townNumber);
                Length += _parent.Distances[CurTown][townNumber];
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
                        if (i != j && !_visited[j])
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

                    throw new Exception("No town to move next");
                }
            }
        }



        public void solve()
        {
            double[][] Distance = CreateArray(n, n);
            double[][] trails = CreateArray(n, n, defaultTrail);

            for (int t = 0; t < max_iteration; t++)
            {
                ants.Clear();
                for (int k = 0; k < m; k++)
                    ants.Add(new Ant(this, random.Next(0, n - 1)));

                for (int k = 0; k < m; k++)
                {
                    ants[k].GoTo(ants[k].NextTown);
                }
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {

                    }
                }
            }


            Console.WriteLine();
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
