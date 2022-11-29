
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

        public List<Pair<int, int>> Loops, Deltas, Whorls, Endings, Bifurcations;

        public SingularityManager(int[,] singularMat, bool[,] msk, int rad) {
            Mat = singularMat;
            // each fingerprint might either have:
            //  - ONE AND ONLY loop, or
            //  - ONE AND ONLY whorl
            Loops = Singularity.GroupType(Mat, Singularity.LOOP);
            Whorls = Singularity.GroupType(Mat, Singularity.WHORL);
            Loops = new() { Singularity.KeepCenterMostGroup(Mat, Singularity.LOOP, Loops) };
            Whorls = new() { Singularity.KeepCenterMostGroup(Mat, Singularity.WHORL, Whorls) };
            //
            if (Loops.Count > 0) {
                Type = Singularity.LOOP;
                Center = Loops.First();
            } else {
                Type = Singularity.WHORL;
                Center = Whorls.First();
            }
            // now modify the Mat & the SegmentMask
            Singularity.CropMask(msk, Center, rad);
            MatTool<int>.Forward(ref Mat, (y, x, v) => {
                if (!msk[y, x])
                    Mat[y, x] = Singularity.NOTYPE;
                return true;
            });
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
