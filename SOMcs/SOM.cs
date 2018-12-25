using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOMcs
{


    public class SOM
    {
        public class Node
        {

            static Random rand;

            /// <summary>
            /// 重みベクトル
            /// </summary>
            public double[] WeightVector { get; private set; }

            /// <summary>
            /// Map上の座標
            /// </summary>
            public Point Coordinate { get; private set; }

            /// <summary>
            /// コンストラクタ.
            /// </summary>
            /// <param name="dimensionNumber">重みベクトルの次元数</param>
            public Node(int dimensionNumber, Point coordinate = new Point(), string label = null, double[] vector = null, int seed = 0)
            {
                if (Node.rand == null)
                {
                    Node.rand = new Random(seed);
                }

                Coordinate = coordinate;
                WeightVector = new double[dimensionNumber];
                if (vector == null)
                {
                    for (int i = 0; i < dimensionNumber; i++)
                    {
                        WeightVector[i] = rand.NextDouble();
                    }
                }
                else
                {
                    vector.CopyTo(WeightVector, 0);
                }

                Label = label;
            }

            public string Label { get; set; }

            public void Update(double[] target, double ratio, string label)
            {
                for (int i = 0; i < WeightVector.Length; i++)
                {
                    WeightVector[i] += ratio  * (target[i] - WeightVector[i]) ;
                }

                Label = label;
            }
        }

        public int Height{ get; private set; }

        public int Width { get; private set;}

        /// <summary>
        /// 初期半径率
        /// </summary>
        public double InitialRadiusRatio { get; private set; }

        /// <summary>
        /// 初期学習率
        /// </summary>
        public double InitialLearningRatio { get; private set; }

        /// <summary>
        /// 学習回数
        /// </summary>
        public int LearningTime{ get; private set; }


        /// <summary>
        /// 出力層のノード
        /// </summary>
        public SOM.Node[,] Nodes { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="height">出力層の縦方向のサイズ</param>
        /// <param name="width">出力層の横方向のサイズ</param>
        /// <param name="initialLearningRatio">初期学習係数</param>
        /// <param name="initRadiusRatio">初期半径率</param>
        /// <param name="learningTime">学習回数</param>
        /// <param name="dimensionNumber">入力ベクトルの次元数</param>
        public SOM(int height = 80, int width = 80, double initialLearningRatio = 0.4, double initRadiusRatio = 0.5, int learningTime = 500 , int dimensionNumber = 3)
        {
            Height = height;
            Width = width;
            Nodes = new Node[Height, Width];
            InitialLearningRatio = initialLearningRatio;
            InitialRadiusRatio = initRadiusRatio;
            LearningTime = learningTime;

            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    if ((h + w) % 2 == 0)
                    {
                        Nodes[h, w] = new Node(dimensionNumber, new Point(w, h));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public bool Learning(List<SOM.Node> nodes)
        {
            bool ret = false;
            Point winnerPoint;
            
            for(int t = 1; t <= LearningTime; t++)
            {
                foreach (Node node in nodes)
                {
                    // 勝者ノードの探索
                    winnerPoint = SearchWinnerNode(node.WeightVector);

                    // 近傍領域の更新
                    Update(node, winnerPoint, t);                
                }
            }

            return ret;
        }

        /// <summary>
        /// 入力ベクトルに一番距離が近いノードの座標を返す
        /// </summary>
        /// <param name="vector">入力ベクトル</param>
        /// <returns></returns>
        private Point SearchWinnerNode(double[] vector)
        {
            double dist = 0xeffffffff;
            Point winnerPoint = new Point(0, 0);
            double buf = 0;

            foreach (Node node in Nodes)
            {
                if (node != null)
                {
                    buf = CalcDistance(vector, node);
                    if (buf <= dist)
                    {
                        dist = buf;
                        winnerPoint = node.Coordinate;
                    }
                }
            }

            return winnerPoint;
        }

        /// <summary>
        /// 入力ベクトルとノード間の距離を計算する。
        /// </summary>
        /// <param name="vector">入力ベクトル</param>
        /// <param name="node">ノード</param>
        /// <returns></returns>
        private double CalcDistance(double[] vector, Node node)
        {
            double dist = 0;

            for(int i = 0; i < node.WeightVector.Length; i++)
            {
                dist += Math.Abs(vector[i] - node.WeightVector[i]);
            }

            return dist;

        }

        /// <summary>
        /// 勝者ノードを含む近傍ノードの更新
        /// </summary>
        /// <param name="target">入力ベクトル</param>
        /// <param name="winnerPoint">勝者ノードの座標</param>
        /// <param name="t">学習回数</param>
        /// <returns></returns>
        private bool Update(Node target, Point winnerPoint, int t)
        {
            bool ret = false;
            int hammingDist = 0;
            double learningRatio = CalcLearningRatio(t);
            double ratio;
            int radius = CalcLearningRadius(t);
            int h_n = winnerPoint.Y - radius;
            int h_p = winnerPoint.Y + radius;
            int w_n = winnerPoint.X - radius;
            int w_p = winnerPoint.X + radius;

            if(h_n < 0)
            {
                h_n = 0;
            }

            if(h_p >= Height)
            {
                h_p = Height - 1;
            }

            if (w_n < 0)
            {
                w_n = 0;
            }

            if (w_p >= Width)
            {
                w_p = Width- 1;
            }

            for (int h = h_n; h <= h_p; h++)
            {
                for (int w = w_n; w <= w_p; w++)
                {
                    hammingDist = Math.Abs(h - winnerPoint.Y) + Math.Abs(w - winnerPoint.X);
                    if (hammingDist <= radius)
                    {
                        if ((h + w) % 2 == 0)
                        {
                            ratio = CalcRatio(hammingDist, learningRatio);
                            Nodes[h, w].Update(target.WeightVector, ratio, target.Label);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 学習回数に応じた近傍領域
        /// </summary>
        /// <param name="t">学習回数</param>
        /// <returns></returns>
        private int CalcLearningRadius(int t)
        {
            int ret =  (int)(InitialRadiusRatio * (Height < Width ? Height : Width) * (1 -   (double)t / LearningTime));
            return ret;
        }

        /// <summary>
        ///学習回数に応じた 学習係数を計算する
        /// </summary>
        /// <param name="t">現在の学習回数</param>
        /// <returns></returns>
        private double CalcLearningRatio(int t)
        {
            double ret = InitialLearningRatio * (1.0 -  (double)t / LearningTime);
            return ret;
        }

        /// <summary>
        /// 勝者ノードとの距離に応じた学習係数を計算する
        /// </summary>
        /// <param name="hammingDist">勝者ノードとの距離</param>
        /// <param name="learningRatio">学習に応じた学習係数</param>
        /// <returns></returns>
        private double CalcRatio(int hammingDist, double learningRatio)
        {
            return learningRatio / (hammingDist  + 1.0);
        }
    }
}
