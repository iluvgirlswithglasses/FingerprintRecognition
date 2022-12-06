
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    internal class BruteComparator {

        FImage A, B;

        /** 
         * @ loss core:
         * 
         * represent the amount of information that one of the fingerprint has
         * but the other does not
         * */

        //
        public double RidgeMismatchScore = 1;
        public double SingularityMismatchScore = 1;

        public Pair<int, int> CenterA = new();
        public Pair<int, int> CenterB = new();

        //
        int UsefulRad;
        int AngleSpanDeg;
        double AngleSpanRad;
        double DistTolerance;

        /** 
         * @ drivers 
         * */
        public BruteComparator(FImage a, FImage b, int usefulRad, int angleSpanDeg, double distTolerance) {
            A = a;
            B = b;

            UsefulRad = usefulRad;
            AngleSpanDeg = angleSpanDeg;
            AngleSpanRad = angleSpanDeg * PI / 180;
            DistTolerance = distTolerance;

            // assume that a pair of core sigularity is the same
            // proceed comparison 
            foreach (var i in A.SingularMgr.CoreSingularLst) {
                foreach (var j in B.SingularMgr.CoreSingularLst) {
                    // if these are of the same kind
                    if (i.Nd == j.Nd) {
                        Compare(i.St, j.St);
                    }
                }
            }

            // in worse case, where the given fingerprint has no viable core singularity
            // proceed to compare two center
            Compare(A.ColorCenter, B.ColorCenter);
        }

        public double CalcScore() {
            return CalcScore(RidgeMismatchScore, SingularityMismatchScore);
        }

        private double CalcScore(double ridge, double singu) {
            return ridge + singu;
        }

        /** 
         * @ compare based on ridges
         * */
        public void Compare(Pair<int, int> a, Pair<int, int> b) {

            var aLoop = ExtractType(A.SingularMgr.CoreSingularLst, Singularity.LOOP, a);
            var aDelta = ExtractType(A.SingularMgr.CoreSingularLst, Singularity.DELTA, a);
            var bLoop = ExtractType(B.SingularMgr.CoreSingularLst, Singularity.LOOP, b);
            var bDelta = ExtractType(B.SingularMgr.CoreSingularLst, Singularity.DELTA, b);

            // deg would not be increased by angleSpan after each iteration
            double offLim = (double)12 / 180 * PI;
            double offInc = (double)1 / 180 * PI;

            // rotate the second fingerprint
            for (double off = -offLim; off <= offLim; off += offInc) {
                int ridgeComparisonCnt = 0;
                double ridgeMismatch = 0;
                int singuComparisonCnt = 0;
                int singuMismatch = 0;
                // 360deg ridge compare
                for (double d = 0; d <= PI * 2; d += AngleSpanRad) {
                    CompareRidge(a, b, d, off, ref ridgeComparisonCnt, ref ridgeMismatch);
                }
                // comparison count threshold
                if (ridgeComparisonCnt > PI / AngleSpanRad) {
                    double rmm = ridgeMismatch / ridgeComparisonCnt;
                    // there's a sufficient amount of ridge
                    // --> proceed to compare relative singularities
                    ComparePointSet(aLoop, bLoop, off, ref singuComparisonCnt, ref singuMismatch);
                    ComparePointSet(aDelta, bDelta, off, ref singuComparisonCnt, ref singuMismatch);

                    double smm = 0;
                    if (singuComparisonCnt > 0)
                        smm = (double)singuMismatch / singuComparisonCnt;

                    // whether to use this new result or not
                    if (CalcScore(rmm, smm) < CalcScore()) {
                        RidgeMismatchScore = rmm;
                        SingularityMismatchScore = smm;

                        CenterA = a;
                        CenterB = b;
                    }
                }
            }
        }

        public void CompareRidge(Pair<int, int> a, Pair<int, int> b, double d, double off, ref int cnt, ref double mm) {
            // VTPT of this line:
            // (Cos(d), -Sin(d))
            int dist = Min(
                GetDist(A.SegmentMask, a, Cos(d), -Sin(d)),
                GetDist(B.SegmentMask, b, Cos(d + off), -Sin(d + off))
            );
            dist = Convert.ToInt32(0.8 * dist);

            // the distance is too insignificant to extract reliable information
            if (dist < 30) return;

            int aCnt = CountRidges(A.Skeleton, a, Cos(d), -Sin(d), dist);
            int bCnt = CountRidges(B.Skeleton, b, Cos(d + off), -Sin(d + off), dist);
            cnt++;
            if (Max(aCnt, bCnt) > 0) {
                mm += (double)Abs(aCnt - bCnt) / Max(aCnt, bCnt);
            }
        }

        // calc the distance between src and the mask
        static private int GetDist(bool[,] msk, Pair<int, int> src, double a, double b) {
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

        // count the number of ridges in the line: `ay + bx`
        static private int CountRidges(bool[,] ske, Pair<int, int> src, double a, double b, int dist) {
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
        private void ComparePointSet(List<Pair<double, double>> a, List<Pair<double, double>> b, double rad, ref int cnt, ref int mmscore) {
            int matches = 0;
            bool[] selected = new bool[b.Count];

            foreach (Pair<double, double> u in a) {
                double uAng = CalcAngle(u);
                for (int i = 0; i < b.Count; i++) {
                    if (!selected[i]) {
                        Pair<double, double> v = b[i];
                        v = RotatePoint(v, -rad);
                        double vAng = CalcAngle(v);
                        
                        double d = Abs(uAng - vAng);
                        if (uAng * vAng < 0) {
                            d = Min(
                                Abs(uAng) + Abs(vAng),
                                Abs(Abs(uAng) - PI) + Abs(Abs(vAng) - PI)
                            );
                        }

                        Console.WriteLine("{0} {1}", u, v);

                        // match
                        if (d <= 45 * PI / 180 && Abs(CalcLen(u) - CalcLen(v)) <= DistTolerance) {
                            matches++;
                            selected[i] = true;
                            break;
                        }
                    }
                }
            }

            cnt += a.Count;
            mmscore += a.Count - matches;
        }

        public List<Pair<double, double>> ExtractType(List<Pair<Pair<int, int>, int>> ls, int t, Pair<int, int> c) {
            List<Pair<double, double>> res = new();
            foreach (var i in ls) {
                if (
                    i.Nd == t && 
                    Abs(i.St.St - c.St) <= UsefulRad && Abs(i.St.Nd - c.Nd) <= UsefulRad &&
                    i.St.St != c.St && i.St.Nd != c.Nd
                ) {
                    res.Add(GetVector(c, i.St));
                }
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
            double rad = Acos(v.Nd / CalcLen(v));
            if (v.St < 0) rad = -rad;
            return rad;
        }
    }
}
