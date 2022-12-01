using FingerprintRecognition.Comparator;

namespace FingerprintRecognition.Comparator {

    public class AllImagesComparators {

        int St, Fi, Cnt;
        FImage[] Imgs;

        List<int> Parent;
        Dictionary<int, List<int>> Group;
        
        // [st: fi)
        public AllImagesComparators(FImage[] imgs, int st, int fi) {
            Imgs = imgs;
            St = st;
            Fi = fi;
            Cnt = Fi - St;

            // for some reason I can't init an array with size
            Parent = new();
            for (int i = 0; i < Cnt; i++)
                Parent.Add(i);
            Group = new();
        }

        public void Compare(bool ss, double sLenTolerance, double sAngleTolerance, double sRidgeTolerance, double aRidgeTolerance) {
            for (int i = St; i < Fi; i++) {
                for (int j = i + 1; j < Fi; j++) {
                    SingularityComparator cmp = new SingularityComparator(
                        Imgs[i].SingularMgr, Imgs[j].SingularMgr, Imgs[i].Skeleton, Imgs[j].Skeleton
                    );
                    Console.WriteLine(
                        "Img {0} & {1}:\nSinguls match = {2}, angle diff = {3}, dist diff = {4}, ridges mismatch score = {5}\n", 
                        i, j, cmp.SMatches, cmp.SAngleMismatchScore, cmp.SLenMismatchScore, cmp.SingulRidgesMismatchScore
                    );
                    if (MatchTolerance(cmp, ss, sLenTolerance, sAngleTolerance, sRidgeTolerance, aRidgeTolerance)) {
                        Join(i - St, j - St);
                    }
                }
            }
            // group matches
            for (int i = 0; i < Cnt; i++) {
                int g = GetParent(i);
                if (Group.ContainsKey(g))
                    Group[g].Add(i + St);
                else
                    Group[g] = new() { i + St };
            }
        }

        public bool MatchTolerance(SingularityComparator cmp, bool ss, double sLenTolerance, double sAngleTolerance, double sRidgeTolerance, double aRidgeTolerance) {
            if (ss && !cmp.SMatches)
                return false;
            return cmp.SLenMismatchScore <= sLenTolerance && 
                   cmp.SAngleMismatchScore <= sAngleTolerance &&
                   cmp.SingulRidgesMismatchScore <= sRidgeTolerance &&
                   cmp.RidgesMismatchScore <= aRidgeTolerance;
        }

        /** @ dsu */
        private int GetParent(int i) {
            if (Parent[i] == i)
                return i;
            Parent[i] = GetParent(Parent[i]);
            return Parent[i];
        }

        private bool Join(int a, int b) {
            int u = GetParent(a), v = GetParent(b);
            if (u == v)
                return false;
            // performance not prioritied
            if (u > v)
                Parent[u] = v;
            else
                Parent[v] = u;
            return true;
        }

        /** 
         * @ printing
         * */ 
        public void PrintGroups() {
            foreach (var g in Group) {
                Console.Write(String.Format("{0}: ", g.Key));
                foreach (var i in g.Value)
                    Console.Write(String.Format("{0} ", i));
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
