
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Comparator {

    /** 
     * this classes manages the "Singular" matrix in FImage
     * */

    public class SingularityManager {

        public int Type = -1;
        public int[,] Mat;
        public Pair<int, int> Center;  // the core coord of the fingerprint

        public List<Pair<int, int>> Loops, Deltas, Whorls, Endings = new(), Bifurcations = new();

        public SingularityManager(int[,] singularMat, bool[,] msk, int rad) {
            Mat = singularMat;
            // each fingerprint might either have:
            //  - ONE or TWO loop, or
            //  - ONE AND ONLY whorl
            Loops = Singularity.GroupType(Mat, Singularity.LOOP);
            Whorls = Singularity.GroupType(Mat, Singularity.WHORL);
            Whorls = new() { Singularity.KeepCenterMostGroup(Mat, Singularity.WHORL, Whorls) };
            //
            if (Whorls.Count > 0) {
                Type = Singularity.WHORL;
                Center = Whorls.First();
            } else {
                Type = Singularity.LOOP;
                Center = Loops.First();
            }
            // and now I can safely extracts other data
            Deltas = Singularity.GroupType(Mat, Singularity.DELTA);
        }

        public void ExtractKeysFromSkeleton(bool[,] skele) {
            Endings = Singularity.DetectEndings(Mat, skele);
            Bifurcations = Singularity.DetectBifurs(Mat, skele);
            foreach (var i in Endings)
                Mat[i.St, i.Nd] = Singularity.ENDING;
            foreach (var i in Bifurcations)
                Mat[i.St, i.Nd] = Singularity.BIFUR;
        }
    }
}
