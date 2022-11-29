
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Comparator {

    public class SingularityManager {

        int Type = -1;
        int[,] SingularMat;
        List<Pair<int, int>> Loops, Deltas, Whorls;
        Pair<int, int> Center;

        public SingularityManager(int[,] singularMat, bool[,] msk, int rad) {
            SingularMat = singularMat;
            // each fingerprint might either have:
            //  - ONE AND ONLY loop, or
            //  - ONE AND ONLY whorl
            Loops = Singularity.GroupType(SingularMat, Singularity.LOOP);
            Whorls = Singularity.GroupType(SingularMat, Singularity.WHORL);
            Loops = new() { Singularity.KeepCenterMostGroup(SingularMat, Singularity.LOOP, Loops) };
            Whorls = new() { Singularity.KeepCenterMostGroup(SingularMat, Singularity.WHORL, Whorls) };
            //
            if (Loops.Count > 0) {
                Type = Singularity.LOOP;
                Center = Loops.First();
            } else {
                Type = Singularity.WHORL;
                Center = Whorls.First();
            }
            // now modify the SingularMat & the SegmentMask
            Singularity.CropMask(msk, Center, rad);
            MatTool<int>.Forward(ref SingularMat, (y, x, v) => {
                if (!msk[y, x])
                    SingularMat[y, x] = Singularity.NOTYPE;
                return true;
            });
            // and now I can safely extracts the delta
            Deltas = Singularity.GroupType(SingularMat, Singularity.DELTA);
        }
    }
}
