using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter {

    public class Singularity {

        public static readonly int DELTA = 0, LOOP = 1, WHORL = 2, ENDING = 3, BIFUR = 4, NOTYPE = -1;

        public static readonly Bgr[] COLORS = {
            new Bgr(0, 0, 255),     // 0: delta, red
            new Bgr(0, 128, 255),   // 1: loop, orange
            new Bgr(255, 128, 255), // 2: whorl, pink
            new Bgr(255, 150, 0),   // 3: ending, cyan
            new Bgr(0, 255, 0)      // 4: bifur, green
        };

        static readonly KeyValuePair<int, int>[] RELATIVE = {
            // this goes round
            new(-1, -1), new(-1, +0), new(-1, +1), new(+0, +1), new(+1, +1), new(+1, +0), new(+1, -1), new(+0, -1), new(-1, -1)
        };

        /**
         * @ detects singularities
         * 
         * height: original img height
         * width: original img width
         * */
        static public int[,] Create(int height, int width, double[,] orient, int w, bool[,] msk) {
            int[,] res = new int[height, width];
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
                // DELTA
                return 0;
            if (-180 - margin <= total && total <= -180 + margin)
                // LOOP
                return 1;
            if (360 - margin <= total && total <= 360 + margin)
                // WHORL
                return 2;
            // NOTYPE
            return -1;
        }

        /** 
         * @ group the singularities
         * 
         * mat: the singularity matrix, size: img_height * img_width
         * */
        static public List<Pair<int, int>> GroupType(int[,] mat, int typ) {

            List<Pair<int, int>> groups = new();
            int[,] group = new int[mat.GetLength(0), mat.GetLength(1)];
            int groupCnt = 1;

            MatTool<int>.Forward(ref group, (y, x, v) => {
                if (v == 0 && mat[y, x] == typ) {
                    groups.Add(BFS(mat, group, groupCnt, typ, y, x));
                    groupCnt++;
                }
                return true;
            });

            return groups;
        }

        /**
         * @ keep the center-most group of the `typ` singularity type
         *   while excluding the others
         * 
         * mat: the singularity matrix, size: img_height * img_width
         * groups: the groups list created the GroupType() function
         * */
        static public Pair<int, int> KeepCenterMostGroup(int[,] mat, int typ, List<Pair<int, int>> groups) {
            Pair<int, int> chosen = new();
            if (groups.Count == 0)
                return chosen;

            Pair<int, int> center = new(mat.GetLength(0) >> 1, mat.GetLength(1) >> 1);
            int minDist = ~0 ^ (1 << 31);

            foreach (var i in groups) {
                int d = Distance(i, center);
                if (d < minDist) {
                    minDist = d;
                    chosen = i;
                }
            }

            int[,] keep = new int[mat.GetLength(0), mat.GetLength(1)];
            BFS(mat, keep, 1, typ, chosen.St, chosen.Nd);

            MatTool<int>.Forward(ref mat, (y, x, v) => {
                if (v == typ && keep[y, x] == 0)
                    mat[y, x] = NOTYPE;
                return true;
            });

            return chosen;
        }

        /** 
         * @ perform BFS, starting from (y, x) coord
         *   this BFS will bleed through every cell whose type matches the required one
         *   and mark these cells in the group[] matrix
         * */
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
         * @ crop the mask around the core of the fingerprint
         * */
        static public void CropMask(bool[,] msk, Pair<int, int> center, int radius) {
            MatTool<bool>.Forward(ref msk, (i, j, v) => {
                if (Abs(i - center.St) > radius || Abs(j - center.Nd) > radius)
                    msk[i, j] = false;
                return true;
            });
        }

        /** 
         * @ detects endings and bifurcations
         *   only performs on cells that aren't whorls, deltas nor loops
         *   
         * mat[]: the singular matrix
         * */
        static public List<Pair<int, int>> DetectEndings(int[,] mat, bool[,] ske) {
            List<Pair<int, int>> res = new();
            MatTool<int>.Forward(ref mat, 1, 1, mat.GetLength(0) - 1, mat.GetLength(1) - 1, (y, x, v) => {
                if (ske[y, x] && GetAdj(ske, y, x) == 2) {
                    res.Add(new(y, x));
                }
                return true;
            });
            return res;
        }

        static public List<Pair<int, int>> DetectBifurs(int[,] mat, bool[,] ske) {
            List<Pair<int, int>> res = new();
            MatTool<int>.Forward(ref mat, 1, 1, mat.GetLength(0) - 1, mat.GetLength(1) - 1, (y, x, v) => {
                if (ske[y, x] && GetShift(ske, y, x) >= 3) {
                    res.Add(new(y, x));
                }
                return true;
            });
            return res;
        }

        // returns how many time the color shifts
        static private int GetShift(bool[,] ske, int y, int x) {
            int shf = 0;
            for (int i = 0; i < 8; i++)
                if (
                    !ske[y + RELATIVE[i].Key, x + RELATIVE[i].Value] &&
                    ske[y + RELATIVE[i + 1].Key, x + RELATIVE[i + 1].Value]
                ) shf++;
            return shf;
        }

        // returns how many 1 cell there are in the 3x3 masks
        static private int GetAdj(bool[,] ske, int y, int x) {
            int cnt = 0;
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (ske[y + i, x + j]) cnt++;
                }
            }
            return cnt;
        }

        /** 
         * @ tools 
         * */
        static private int Distance(Pair<int, int> a, Pair<int, int> b) {
            return (a.St - b.St)*(a.St - b.St) + (b.Nd - b.Nd)*(b.Nd - b.Nd);
        }
    }
}
