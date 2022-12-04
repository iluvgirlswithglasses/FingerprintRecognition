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
         * @ compare all fingerprints in [Fi:St) using BruteComparator
         * 
         * 
         * */
        public void BruteCompare(int usefulRad, int angleSpan, double distTolerance, double acceptedScore) {
            for (int i = St; i < Fi; i++) {
                for (int j = i + 1; j < Fi; j++) {
                    /*int u = GetParent(i - St), v = GetParent(j - St);
                    if (u == v) continue;*/

                    BruteComparator cmp = new(Imgs[i], Imgs[j], usefulRad, angleSpan, distTolerance);
                    Console.WriteLine(String.Format("Comparing {0} and {1}: Bifur MMScore = {2} [{3}]", i, j, cmp.BifurMismatchScore, cmp.BifurMismatchScore < acceptedScore));
                    if (cmp.BifurMismatchScore < acceptedScore) {
                        Join(i - St, j - St);
                    }
                }
            }
        }

        /** 
         * @ compare all fingerprints in [Fi:St) using SingularityComparator
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
                    bool accepted = MatchTolerance(cmp, ss, angleTolerance, sLenTolerance, sAngleTolerance, sRidgeTolerance);
                    if (accepted) {
                        Join(i - St, j - St);
                    }

                    Console.WriteLine(
                        "Img {0} & {1}: {2}\nSinguls match = {3}, angle diff = {4}, s angle diff = {5}, dist diff = {5}, ridges mismatch score = {6}\n", 
                        i, j, accepted, cmp.SMatches, cmp.AngleDiff, cmp.SAngleMismatchScore, cmp.SLenMismatchScore, cmp.SingulRidgesMismatchScore
                    );
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
