
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    internal class BruteComparator {

        FImage A, B;

        // represent the amount of information that one of the fingerprint has
        // but the other does not
        public double LossScore;
        //
        public double BifurMismatchScore = 1;
        public double EndingMismatchScore;
        public double RidgeMismatchScore;
        //
        public double SingularityMismatchScore;

        //
        int UsefulRad;
        int AngleSpan;
        double DistTolerance;

        /** 
         * @ drivers 
         * */
        public BruteComparator(FImage a, FImage b, int usefulRad, int angleSpan, double distTolerance) {
            A = a;
            B = b;

            UsefulRad = usefulRad;
            AngleSpan = angleSpan;
            DistTolerance = distTolerance;

            foreach (var i in A.SingularMgr.CoreSingularLst) {
                foreach (var j in B.SingularMgr.CoreSingularLst) {
                    // if these are of the same kind
                    if (i.Nd == j.Nd) {
                        CompareCorepoint(i.St, j.St);
                        CompareRidge(i.St, j.St);
                    }
                }
            }
        }

        /** 
         * @ compare based on ridges
         * */
        static public void CompareRidge(Pair<int, int> a, Pair<int, int> b) {
            // deg would not be increased by angleSpan after each iteration
            double angLim = (double)12 / 180 * PI;
            double angInc = (double)1 / 180 * PI;

            // rotate the second fingerprint
            for (double ang = -angLim; ang <= angLim; ang += angInc) {
                // the base line equation of `b`
                // (fa, fb) = (y, x)
                double fa = Cos(ang), fb = -Sin(ang);
            }
        }



        // calc the distance between src and the mask
        static public int GetDist(bool[,] msk, Pair<int, int> src, double a, double b) {
            int dist = 0; 
            double c = - a*src.St - b*src.Nd;

            if (Abs(b) > Abs(a)) {
                // Sin(alpha) > Cos(alpha)
                // --> y diff > x diff
                int yInc = 1;
                if (b > 0)  // b = -Sin(alpha)
                    yInc = -1;
                for (int y = src.St; 0 <= y && y < msk.GetLength(0); y += yInc) {
                    int x = (int)Round((-a * y - c) / b);
                    if (x < 0 || msk.GetLength(1) <= x || !msk[y, x])
                        break;
                    dist++;
                }
            }
            else {
                // y diff <= x diff
                int xInc = 1;
                if (a < 0)  // a = Cos(alpha)
                    xInc = -1;
                for (int x = src.Nd; 0 <= x && x < msk.GetLength(1); x += xInc) {
                    int y = (int)Round((-b * x - c) / a);
                    if (y < 0 || msk.GetLength(0) <= y || !msk[y, x])
                        break;
                    dist++;
                }
            }

            return dist;
        }

        // count the number of ridges in the line:
        //      ay + bx
        static public int CountRidges(Pair<int, int> src, double a, double b, int dist, bool[,] ske) {
            int cnt = 0;
            bool pre = false;

            double c = - a*src.St - b*src.Nd;

            if (Abs(b) > Abs(a)) {
                // Sin(alpha) > Cos(alpha)
                // --> y diff > x diff
                int yInc = 1;
                if (b > 0)  // b = -Sin(alpha)
                    yInc = -1;
                for (int y = src.St; 0 <= y && y < ske.GetLength(0) && Abs(y - src.St) <= dist; y += yInc) {
                    int x = (int)Round((-a * y - c) / b);
                    if (x < 0 || ske.GetLength(1) <= x)
                        break;
                    //
                    if (ske[y, x] && !pre)
                        cnt++;
                    pre = ske[y, x];
                }
            } else {
                // y diff <= x diff
                int xInc = 1;
                if (a < 0)  // a = Cos(alpha)
                    xInc = -1;
                for (int x = src.Nd; 0 <= x && x < ske.GetLength(1) && Abs(x - src.Nd) <= dist; x += xInc) {
                    int y = (int)Round((-b * x - c) / a);
                    if (y < 0 || ske.GetLength(1) <= y)
                        break;
                    //
                    if (ske[y, x] && !pre)
                        cnt++;
                    pre = ske[y, x];
                }
            }
            return cnt;
        }

        /** 
         * @ compare based on singularities 
         * */
        // every point within angleSpan (measured in degree)
        // is consider to be in a straight line
        private void CompareCorepoint(Pair<int, int> a, Pair<int, int> b) {
            var aBifur = ToRelativePosition(
                ExtractType(A.SingularMgr.SingularLst, Singularity.BIFUR, a), 0
            );
            var aEnding = ToRelativePosition(
                ExtractType(A.SingularMgr.SingularLst, Singularity.ENDING, a), 0
            );

            var bBifurAbs = ExtractType(B.SingularMgr.SingularLst, Singularity.BIFUR, b);
            var bEndingAbs = ExtractType(B.SingularMgr.SingularLst, Singularity.ENDING, b);

            // deg would not be increased by angleSpan after each iteration
            double angLim = (double) 12 / 180 * PI;
            double angInc = (double) 1 / 180 * PI;

            // rotate the second fingerprint
            for (double ang = -angLim; ang <= angLim; ang += angInc) {
                var bBifur = ToRelativePosition(bBifurAbs, ang);
                var bEnding = ToRelativePosition(bEndingAbs, ang);

                // check out how this rotation works
                int bifurCnt = 0, bifurMismatch = 0;
                ComparePointSet(aBifur, bBifur, ref bifurCnt, ref bifurMismatch);
                if (bifurCnt > 0)
                    BifurMismatchScore = Min(BifurMismatchScore, (double)bifurMismatch / bifurCnt);
            }
        }

        private void ComparePointSet(Dictionary<int, SortedSet<double>> a, Dictionary<int, SortedSet<double>> b, ref int cnt, ref int mmscore) {
            foreach (var i in a.Keys) {
                foreach (double u in a[i]) {
                    cnt++;
                    // C# is TERRIBLE
                    var s = b[i].GetViewBetween(u - DistTolerance, u + DistTolerance);
                    if (s.Count == 0) {
                        mmscore++;
                        continue;
                    }
                    //
                    double v = 1e9+7;
                    foreach (var pt in s) {
                        if (Abs(u - pt) < Abs(u - v))
                            v = pt;
                    }
                    b[i].Remove(v);
                }
            }
        }

        /** 
         * @ tools
         * */
        public List<Pair<double, double>> ExtractType(List<Pair<Pair<int, int>, int>> ls, int t, Pair<int, int> c) {
            List<Pair<double, double>> res = new();
            foreach (var i in ls) {
                if (i.Nd == t && Abs(i.St.St - c.St) <= UsefulRad && Abs(i.St.Nd - c.Nd) <= UsefulRad) {
                    res.Add(GetVector(c, i.St));
                }
            }
            return res;
        }

        public Dictionary<int, SortedSet<double>> ToRelativePosition(List<Pair<double, double>> pos, double rad) {
            Dictionary<int, SortedSet<double>> res = new();
            for (int i = 0; i <= 360; i += AngleSpan)
                res.Add(i, new());
            foreach (var p in pos) {
                double len = CalcLen(p);
                // the angle between vector (y=0, x=1) and vector p
                double ang = Acos(p.Nd / len);
                double deg = ang * 180 / PI;
                if (p.Nd < 0)
                    deg = 360 - deg;
                int d = ((int)Floor(deg) / AngleSpan) * AngleSpan;
                res[d].Add(len);

                Console.WriteLine(String.Format("{0} - {1}", p, d));
            }

            return res;
        }

        /** 
         * @ math tools
         * */
        static private Pair<double, double> GetVector(Pair<int, int> a, Pair<int, int> b) {
            return new(b.St - a.St, b.Nd - a.Nd);
        }

        static private Pair<double, double> GetVector(Pair<double, double> a, Pair<double, double> b) {
            return new(b.St - a.St, b.Nd - a.Nd);
        }

        static private Pair<double, double> RotatePoint(Pair<double, double> p, double rad) {
            double y = p.Nd * Sin(rad) + p.St * Cos(rad);
            double x = p.Nd * Cos(rad) - p.St * Sin(rad);
            return new(y, x);
        }

        static private double CalcLen(Pair<double, double> v) {
            return Sqrt(v.St * v.St + v.Nd * v.Nd);
        }

        static private double CalcAngle(Pair<double, double> v) {
            return Atan2(v.St, v.Nd);
        }
    }
}
