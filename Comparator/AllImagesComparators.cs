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

            // why does this look so freaking heretic
            Parent = new(new int[Cnt]);
            for (int i = 0; i < Cnt; i++)
                Parent[i] = i;
            Group = new();
        }

        /** 
         * @ compare all fingerprints in [Fi:St)
         * 
         * ss: whether or not all significant singularities of two fingerprint must match
         * angleTolerance: the first two singularities' angle tolerance
         * sLenTolerance: singularities distance mismatches mismatch threshold
         * sAngleTolerance: singularities angle mismatches threshold
         * */
        public void Compare(bool ss, double angleTolerance, double sLenTolerance, double sAngleTolerance, double sRidgeTolerance) {
            for (int i = St; i < Fi; i++) {
                for (int j = i + 1; j < Fi; j++) {
                    SingularityComparator cmp = new SingularityComparator(
                        Imgs[i], Imgs[j]
                    );
                    Console.WriteLine(
                        "Img {0} & {1}:\nSinguls match = {2}, angle diff = {3}, dist diff = {4}, ridges mismatch score = {5}\n", 
                        i, j, cmp.SMatches, cmp.SAngleMismatchScore, cmp.SLenMismatchScore, cmp.SingulRidgesMismatchScore
                    );
                    if (MatchTolerance(cmp, ss, angleTolerance, sLenTolerance, sAngleTolerance, sRidgeTolerance)) {
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

        public bool MatchTolerance(SingularityComparator cmp, bool ss, double angleTolerance, double sLenTolerance, double sAngleTolerance, double sRidgeTolerance) {
            if (ss && !cmp.SMatches)
                return false;
            return cmp.AngleDiff <= angleTolerance &&
                   cmp.SLenMismatchScore <= sLenTolerance && 
                   cmp.SAngleMismatchScore <= sAngleTolerance &&
                   cmp.SingulRidgesMismatchScore <= sRidgeTolerance;
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
            int groupCnt = 0;
            foreach (var g in Group) {
                Console.Write(String.Format("{0}: ", groupCnt++));
                foreach (var i in g.Value)
                    Console.Write(String.Format("{0} ", i));
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
