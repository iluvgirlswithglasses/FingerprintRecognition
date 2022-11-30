using FingerprintRecognition.DataStructure;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    public class SingularityComparator {

        public bool SMatches = false;   // if all singularities matches

        public double SLenMismatchScore = 0.0;      // singularities distance mismatches
        public double SAngleMismatchScore = 0.0;    // singularities angle mismatches

        public double SingulRidgesMismatchScore = 0.0;  // ridges between core singularities
        public double RidgesMismatchScore = 0.0;        // arbitrary ridges score

        public int ASCnt;       // how many singularities are there in fingerprint A
        public double ASLen;    // the distance between the first two singularities of A
        public double BSLen;    // the distance between the first two singularities of B

        // let alpha be the angle of the vector that connect the st & the nd singu of A
        // let  beta ------------------------------- connect the st & the nd singu of B
        // AngleDiff is beta - alpha
        public double AngleDiff;

        public SingularityComparator(
            SingularityManager mgrA, SingularityManager mgrB, bool[,] skeA, bool[,] skeB
        ) {
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
            if (ASCnt == 1) {
                SMatches = true;
            } else {
                SMatches = CompareSingularity(mgrA.SingularLst, mgrB.SingularLst);
            }

            // perform endings, bifurs, and ridges count comparison here
            if (SMatches) {
                CompareRidges(mgrA.SingularLst, mgrB.SingularLst, skeA, skeB);
            }
        }

        // assert that ASCnt == BSCnt && ASCnt >= 2
        // a[i]: { the position of the `i-th` singularity, the type of that singularity }
        private bool CompareSingularity(List<Pair<Pair<int, int>, int>> a, List<Pair<Pair<int, int>, int>> b) {
            for (int i = 0; i < ASCnt; i++)
                // these core points does not allign
                if (a[i].Nd != b[i].Nd)
                    return false;
            if (ASCnt == 1)
                return true;
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
        private void CompareRidges(List<Pair<Pair<int, int>, int>> a, List<Pair<Pair<int, int>, int>> b, bool[,] skeA, bool[,] skeB) {
            if (ASCnt == 1) {
                // brute-force rotate comparison

            } else {
                for (int i = 0; i < ASCnt - 1; i++) {
                    int aCnt = CountRidges(a[i].St, a[i + 1].St, skeA);
                    int bCnt = CountRidges(b[i].St, b[i + 1].St, skeB);
                    SingulRidgesMismatchScore += (double)Abs(aCnt - bCnt) / Max(aCnt, bCnt);
                }

                // might enhances this somehow later
            }
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
