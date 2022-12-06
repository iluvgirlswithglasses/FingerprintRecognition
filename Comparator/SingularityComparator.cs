using FingerprintRecognition.DataStructure;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    /***/

    public class SingularityComparator {

        public bool SMatches = false;   // if all singularities matches

        public double SLenMismatchScore = 0.0;      // singularities distance mismatches
        public double SAngleMismatchScore = 0.0;    // singularities angle mismatches

        public double SingulRidgesMismatchScore = 0.0;  // ridges between core singularities
        public double BifurRidgesMismatchScore = 0.0;   // mismatch score between core singularities and bifurs
        public double EndingRidgesMismatchScore = 0.0;  // mismatch score between core singularities and endings

        public int ASCnt;       // how many singularities are there in fingerprint A
        public double ASLen;    // the distance between the first two singularities of A
        public double BSLen;    // the distance between the first two singularities of B

        // let alpha be the angle of the vector that connect the st & the nd singu of A
        // let  beta ------------------------------- connect the st & the nd singu of B
        // AngleDiff is beta - alpha
        public double AngleDiff;

        public SingularityComparator(
            FImage a, FImage b
        ) {
            SingularityManager mgrA = a.SingularMgr, mgrB = b.SingularMgr;
            bool[,] skeA = a.Skeleton, skeB = b.Skeleton;

            if (
                mgrA.Loops.Count != mgrB.Loops.Count || 
                mgrA.Whorls.Count != mgrB.Whorls.Count || 
                mgrA.Deltas.Count != mgrB.Deltas.Count
            ) {
                SMatches = false;
                return;
            }

            // assert core singularities count of A and B are the same
            ASCnt = mgrA.SingularLst.Count;
            if (ASCnt <= 1) {
                SMatches = true;
            } else {
                SMatches = CompareSingularity(mgrA.SingularLst, mgrB.SingularLst);
            }

            // ridges comparision
            CompareRidges(mgrA, mgrB, skeA, skeB, a.UsefulRadius * 0.5);
        }

        // assert that ASCnt == BSCnt && ASCnt >= 2
        // a[i]: { the position of the `i-th` singularity, the type of that singularity }
        private bool CompareSingularity(List<Pair<Pair<int, int>, int>> a, List<Pair<Pair<int, int>, int>> b) {
            for (int i = 0; i < ASCnt; i++)
                // these core points does not allign
                if (a[i].Nd != b[i].Nd)
                    return false;
            //
            Pair<double, double> u = GetVector(a[0].St, a[1].St),
                                 v = GetVector(b[0].St, b[1].St);
            ASLen = CalcLen(u);
            BSLen = CalcLen(v);
            AngleDiff = CalcAlpha(v) - CalcAlpha(u);
            //
            for (int i = 0; i < ASCnt - 1; i++) {
                u = GetVector(a[i].St, a[i + 1].St);
                v = GetVector(b[i].St, b[i + 1].St);
                SLenMismatchScore += Abs(CalcLen(u) / ASLen - CalcLen(v) / BSLen);
                //   (a - a0) - (b - b0)
                // = a - b - a0 + b0
                // = a - b + AngleDiff
                SAngleMismatchScore += Abs(CalcAlpha(u) - CalcAlpha(v) + AngleDiff);
            }
            SLenMismatchScore /= ASCnt - 1;
            SAngleMismatchScore /= ASCnt - 1;
            return true;
        }

        // assert that ASCnt == BSCnt
        // a[i]: { the position of the `i-th` singularity, the type of that singularity }
        private void CompareRidges(SingularityManager a, SingularityManager b, bool[,] skeA, bool[,] skeB, double usefulRad) {
            if (SMatches && ASCnt >= 2) {
                // compare the ridges between significant singularities
                for (int i = 0; i < ASCnt - 1; i++) {
                    int aCnt = CountRidges(a.SingularLst[i].St, a.SingularLst[i + 1].St, skeA);
                    int bCnt = CountRidges(b.SingularLst[i].St, b.SingularLst[i + 1].St, skeB);
                    SingulRidgesMismatchScore += (double)Abs(aCnt - bCnt) / Max(aCnt, bCnt);
                }
            }

            // if SMatches == 0,
            // we assume a pair of singulartities is the same
            // and perform comparision based on the pair

            BifurRidgesMismatchScore = 1.0;
            EndingRidgesMismatchScore = 1.0;

            for (int _i = 0; _i < a.SingularLst.Count; _i++) {
                for (int _j = _i; _j < b.SingularLst.Count; _j++) {
                    Pair<Pair<int, int>, int> i = a.SingularLst[_i],
                                              j = b.SingularLst[_j];
                    
                    // even the type is different
                    // --> this can not be the same singularity
                    if (i.Nd != j.Nd)
                        continue;

                    double crBifur = 0.0, crEnding = 0.0;
                    Pair<int, int> aLoc = i.St, bLoc = j.St;

                    /** @ compare the ridges between significant singularities and bifurs */
                    // bifurs of 'A', allign around this singularies


                    // try to rotate `B` and get the best result
                    double limRad = 30 * PI / 180, radSpan = 8 * PI / 180;
                    for (double rad = -limRad; rad <= limRad; rad += radSpan) {

                    }

                    // store the result
                    BifurRidgesMismatchScore = Min(BifurRidgesMismatchScore, crBifur);
                }
            }
        }

        private void ToRelativePos(List<Pair<double, double>> ls, double degSpan) {
            // ls[i] = {y, x}
            List<List<double>> res = new();
        }

        private double RotationalKeypointCompare(List<Pair<double, double>> a, List<Pair<double, double>> b) {
            return 0.0;
        }

        /** 
         * @ shortcuts
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

        static private double CalcAlpha(Pair<double, double> v) {
            return Atan2(v.St, v.Nd);
        }

        /** 
         * @ compare ridges' geometric calculator
         * */
        static public int CountRidges(Pair<int, int> st, Pair<int, int> fi, bool[,] ske) {
            int cnt = 0;
            bool pre = false;

            Pair<double, double> v = GetVector(fi, st);
            v = new(-v.Nd, v.St);   // {y, x}
            double a = v.Nd, b = v.St, c = - a * st.Nd - b * st.St;

            if (Abs(st.St - fi.St) > Abs(st.Nd - fi.Nd)) {
                // y diff > x diff
                for (int y = Min(st.St, fi.St); y <= Max(st.St, fi.St); y++) {
                    int x = (int)Round((- b * y - c) / a);
                    //
                    if (ske[y, x] && !pre)
                        cnt++;
                    pre = ske[y, x];
                }
            } else {
                // y diff <= x diff
                for (int x = Min(st.Nd, fi.Nd); x <= Max(st.Nd, fi.Nd); x++) {
                    int y = (int)Round((- a * x - c) / b);
                    //
                    if (ske[y, x] && !pre) 
                        cnt++;
                    pre = ske[y, x];
                }
            }
            return cnt;
        }
    }
}
