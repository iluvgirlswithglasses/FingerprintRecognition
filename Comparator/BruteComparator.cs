
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
         * 
         * 
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
                    }
                }
            }
        }

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
                    Console.WriteLine("here\n");
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
                double ang = Acos((double)(p.St + p.Nd) / len);
                double deg = ang * 180 / PI;
                if (p.Nd < 0)
                    deg = 360 - deg;
                int d = ((int)Floor(deg) / AngleSpan) * AngleSpan;
                res[d].Add(len);
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
