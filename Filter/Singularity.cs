using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter {

    public class Singularity {

        public static readonly Bgr[] COLORS = {
            new Bgr(0, 0, 255),     // 0: whorl, red
            new Bgr(0, 128, 255),   // 1: delta, orange
            new Bgr(255, 128, 255), // 2: loop, pink
        };

        static readonly KeyValuePair<int, int>[] RELATIVE = {
            // this goes round
            new(-1, -1), new(-1, +0), new(-1, +1), new(+0, +1), new(+1, +1), new(+1, +0), new(+1, -1), new(+0, -1), new(-1, -1)
        };

        /**
         * @ detects singularities
         * */
        static public int[,] Create(bool[,] ske, double[,] orient, int w, bool[,] msk) {
            int[,] res = new int[ske.GetLength(0), ske.GetLength(1)];
            MatTool<int>.Forward(ref res, (y, x, v) => {
                res[y, x] = -1;
                return true;
            });
            //
            Iterator2D.Forward(3, 3, orient.GetLength(0) - 3, orient.GetLength(1) - 3, (y, x) => {
                int t = (y - 2) * w, 
                    l = (x - 2) * w, 
                    d = (y + 3) * w, 
                    r = (x + 3) * w;
                int mskSum = 0;
                MatTool<bool>.Forward(ref msk, t, l, d, r, (_y, _x, v) => {
                    if (v) mskSum++;
                    return true;
                });
                // if all of this region is inside the mask
                if (mskSum == (5*w) * (5*w)) {
                    int typ = PoinCare(orient, y, x, 1);
                    Iterator2D.Forward(y * w, x * w, y * w + w, x * w + w, (i, j) => {
                        res[i, j] = typ;
                        return true;
                    });
                }
                return true;
            });
            return res;
        }

        static public int PoinCare(double[,] orient, int y, int x, int margin) {
            double[] surroundingAngle = new double[9];
            for (int i = 0; i < 9; i++)
                surroundingAngle[i] = orient[y + RELATIVE[i].Key, x + RELATIVE[i].Value] / PI * 180;
            double total = 0;
            for (int i = 0; i < 8; i++) {
                double diff = surroundingAngle[i] - surroundingAngle[i+1];
                if (diff > 90)
                    diff -= 180;
                else if (diff < -90)
                    diff += 180;
                total += diff;
            }

            // COLORS[i]: the color of code `i`
            if (180 - margin <= total && total <= 180 + margin)
                return 0;
            if (-180 - margin <= total && total <= -180 + margin)
                return 1;
            if (360 - margin <= total && total <= 360 + margin)
                return 2;
            return -1;
        }

        /** 
         * @ keeps the most significant singularities
         * */
        static public Pair<int, int> KeepCenterMost(int[,] mat, int typ) {
            // 1-indexed array
            List<Pair<int, int>> groups = new();
            groups.Add(new());

            int[,] group = new int[mat.GetLength(0), mat.GetLength(1)];
            int groupCnt = 1;

            MatTool<int>.Forward(ref group, (y, x, v) => {
                if (v == 0 && mat[y, x] == typ) {
                    groups.Add(BFS(mat, group, groupCnt, typ, y, x));
                    groupCnt++;
                }
                return true;
            });

            Pair<int, int> center = new(mat.GetLength(0) >> 1, mat.GetLength(1) >> 1);
            int chosenGroup = 0;
            int minDist = ~0 ^ (1 << 31);
            for (int i = 1; i < groupCnt; i++) {
                int d = Distance(groups[i], center);
                if (d < minDist) {
                    chosenGroup = i;
                    minDist = d;
                }
            }

            // remove insignificant ones
            MatTool<int>.Forward(ref mat, (y, x, v) => {
                if (v == typ && group[y, x] != chosenGroup)
                    mat[y, x] = -1;
                return true;
            });

            // return the center of the most significant point
            return groups[chosenGroup];
        }

        static private Pair<int, int> BFS(int[,] mat, int[,] group, int currentGroup, int typ, int y, int x) {

            Deque<Pair<int, int>> q = new();
            Pair<double, double> res = new(0, 0);
            int cnt = 0;

            q.AddToBack(new(y, x));
            group[y, x] = currentGroup;

            while (q.Count > 0) {
                Pair<int, int> cr = q.First();
                q.RemoveFromFront();

                cnt++;
                res.St += cr.St;
                res.Nd += cr.Nd;

                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        int a = cr.St + i, b = cr.Nd + j;
                        if (mat[a, b] == typ && group[a, b] == 0) {
                            q.AddToBack(new Pair<int, int>(a, b));
                            group[a, b] = currentGroup;
                        }
                    }
                }
            }

            return new Pair<int, int>((int)Round(res.St / cnt), (int)Round(res.Nd / cnt));
        }

        /** 
         * @ tools 
         * */
        static private int Distance(Pair<int, int> a, Pair<int, int> b) {
            return (a.St - b.St)*(a.St - b.St) + (b.Nd - b.Nd)*(b.Nd - b.Nd);
        }
    }
}
