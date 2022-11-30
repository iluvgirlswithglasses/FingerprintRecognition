using FingerprintRecognition.DataStructure;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    public class SingularityComparator {

        // if all singularities matches
        public bool SMatches = false;
        //
        public double SLenMismatchScore = 0.0;
        public double SAngleMismatchScore = 0.0;

        public int ASCnt;   // how many singularities are there in fingerprint A
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

            ASCnt = mgrA.SingularLst.Count;
            if (ASCnt == 1) {
                SMatches = true;
            } else {
                SMatches = CompareSingularity(mgrA.SingularLst, mgrB.SingularLst);
            }

            // perform endings, bifurs, and ridges count comparison here
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
            for (int i = 1; i < ASCnt - 1; i++) {
                u = GetVector(a[i].St, a[i + 1].St);
                v = GetVector(b[i].St, b[i + 1].St);
                SLenMismatchScore += Abs(CalcLen(u) / ASLen - CalcLen(v) / BSLen);
                //   (a - a0) - (b - b0)
                // = a - b - a0 + b0
                // = a - b + AngleDiff
                SAngleMismatchScore += Abs(CalcAlpha(u) - CalcAlpha(v) + AngleDiff);
            }
            return true;
        }

        /** 
         * @ shortcuts
         * */
        private Pair<double, double> GetVector(Pair<int, int> a, Pair<int, int> b) {
            return new(b.St - a.St, b.Nd - a.Nd);
        }

        private Pair<double, double> GetVector(Pair<double, double> a, Pair<double, double> b) {
            return new(b.St - a.St, b.Nd - a.Nd);
        }

        private double CalcLen(Pair<double, double> v) {
            return Sqrt(v.St * v.St + v.Nd * v.Nd);
        }

        private double CalcAlpha(Pair<double, double> v) {
            return Atan2(v.St, v.Nd);
        }
    }
}
