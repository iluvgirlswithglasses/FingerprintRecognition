
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

        public List<Pair<int, int>> Loops, Deltas, Whorls, Endings = new(), Bifurcations = new();

        // SingularLst[i]: { the position of the `i-th` singularity, the type of that singularity }
        public List<Pair<Pair<int, int>, int>> SingularLst = new(), CoreSingularLst = new();

        public SingularityManager(int[,] singularMat) {
            Mat = singularMat;
            
            // each fingerprint might either have:
            //  - ONE or TWO loop, or
            //  - ONE AND ONLY whorl
            Loops = Singularity.GroupType(Mat, Singularity.LOOP);
            Whorls = Singularity.GroupType(Mat, Singularity.WHORL);
            
            if (Whorls.Count > 0)
                Type = Singularity.WHORL;
            else
                Type = Singularity.LOOP;

            // and now I can safely extracts other data
            Deltas = Singularity.GroupType(Mat, Singularity.DELTA);

            /* uncomment this for SingularityComparator
            foreach (var i in Deltas) SingularLst.Add(new(i, Singularity.DELTA));
            foreach (var i in Loops) SingularLst.Add(new(i, Singularity.LOOP));
            foreach (var i in Whorls) SingularLst.Add(new(i, Singularity.WHORL));
            */
            foreach (var i in Loops) CoreSingularLst.Add(new(i, Singularity.LOOP));
            if (CoreSingularLst.Count == 0) 
                foreach (var i in Deltas) CoreSingularLst.Add(new(i, Singularity.DELTA));
            if (CoreSingularLst.Count == 0)
                foreach (var i in Whorls) CoreSingularLst.Add(new(i, Singularity.WHORL));

            // uncomment this for SingularityComparator
            // SortSingularLst(SingularLst);
        }

        public void ExtractKeysFromSkeleton(bool[,] skele, bool[,] msk, int margin) {
            Endings = Singularity.DetectEndings(Mat, skele, msk, margin);
            Bifurcations = Singularity.DetectBifurs(Mat, skele);
            foreach (var i in Endings) {
                Mat[i.St, i.Nd] = Singularity.ENDING;
                SingularLst.Add(new(i, Singularity.ENDING));
            }
            foreach (var i in Bifurcations) {
                Mat[i.St, i.Nd] = Singularity.BIFUR;
                SingularLst.Add(new(i, Singularity.BIFUR));
            }   
        }

        public void SortSingularLst(List<Pair<Pair<int, int>, int>> ls) {
            ls.Sort((a, b) => {
                Pair<int, int> x = a.St, y = b.St;
                if (x.St == y.St) {
                    if (x.Nd == y.Nd) {
                        return a.Nd - b.Nd;
                    }
                    return x.Nd - y.Nd;
                }
                return x.St - y.St;
            });
        }
    }
}
