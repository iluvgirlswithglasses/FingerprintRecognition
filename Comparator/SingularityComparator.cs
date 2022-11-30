using FingerprintRecognition.DataStructure;
using static System.Math;

namespace FingerprintRecognition.Comparator {

    public class SingularityComparator {

        // if all singularities matches
        public bool SMatches = false;

        public SingularityComparator(SingularityManager mgrA, SingularityManager mgrB, bool[,] skeA, bool[,] skeB) {
            if (
                mgrA.Loops.Count != mgrB.Loops.Count || 
                mgrA.Whorls.Count != mgrB.Whorls.Count || 
                mgrA.Deltas.Count != mgrB.Deltas.Count
            ) {
                SMatches = false;
            } else {
                
            }
        }

        
    }
}
