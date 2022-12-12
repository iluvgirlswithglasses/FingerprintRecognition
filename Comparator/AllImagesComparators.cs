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
         * and group them via DSU
         * */
        public void Compare(int angleSpanDeg, double acceptedScore) {
            for (int i = St; i < Fi; i++) {
                for (int j = i + 1; j < Fi; j++) {
                    int u = GetParent(i - St), v = GetParent(j - St);

                    if (u != i - St || v != j - St) continue;  // i or j already has a group

                    BruteComparator cmp = new(Imgs[u + St], Imgs[j], angleSpanDeg);
                    Console.WriteLine(String.Format(
                        "Comparing {0} and {1}:\nCA = {2}, CB = {3};\nRidge MMScore = {4}; Singu MMScore = {5};\nScore = {6} [{7}]\n", 
                        i, j,
                        cmp.CenterA, cmp.CenterB,
                        cmp.RidgeMismatchScore, cmp.SingularityMismatchScore,
                        cmp.CalcScore(), cmp.CalcScore() <= acceptedScore
                    ));
                    if (cmp.CalcScore() <= acceptedScore) {
                        Join(i - St, j - St);
                    }
                }
            }
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

        public void MakeGroup() {
            // group matches
            for (int i = 0; i < Cnt; i++) {
                int g = GetParent(i);
                if (Group.ContainsKey(g))
                    Group[g].Add(i + St);
                else
                    Group[g] = new() { i + St };
            }
        }

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
