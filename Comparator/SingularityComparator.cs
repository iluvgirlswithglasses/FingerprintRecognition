using FingerprintRecognition.DataStructure;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    public class SingularityComparator {

        // if all singularities matches
        public bool SMatches = false;

        public int ASCnt;   // how many singularities are there in fingerprint A
        public double ASLen;    // the distance between the first two singularities of A
        public double BSLen;    // the distance between the first two singularities of B

        public SingularityComparator(
            SingularityManager mgrA, SingularityManager mgrB, bool[,] skeA, bool[,] skeB,
            double distanceTolerance, double angleTolerence
        ) {
            if (
                mgrA.Loops.Count != mgrB.Loops.Count || 
                mgrA.Whorls.Count != mgrB.Whorls.Count || 
                mgrA.Deltas.Count != mgrB.Deltas.Count
            ) {
                SMatches = false;
                return;
            }

            ASCnt = mgrA.Loops.Count + mgrA.Deltas.Count + mgrA.Whorls.Count;
            if (ASCnt == 1) {
                SMatches = true;
            } else {

            }

            // perform further comparison here
        }

        
    }
}
