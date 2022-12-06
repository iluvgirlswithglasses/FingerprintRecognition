using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Tool;

/** 
* sometimes, some parts of an image are so noisy
* that the singularity extractor get things wrong
* 
* this program is then created to somewhat fix the mess
* */
namespace FingerprintRecognition.Filter {

    public class SingularityCleaner {

        // 
        public static void ExcludeMixedPart(int[,] mat) {
            int h = mat.GetLength(0), w = mat.GetLength(1);
            bool[,] visited = new bool[h, w];
            MatTool<bool>.FromOuterBFSSetup(ref visited, (y, x) => { return visited[y, x] = true; });

            Iterator2D.Forward(h, w, (y, x) => {
                if (!visited[y, x] && mat[y, x] != -1) {
                    ExcludeMixedPartBFS(y, x, visited, mat);
                }
                return true;
            });
        }

        private static void ExcludeMixedPartBFS(int y, int x, bool[,] visited, int[,] mat) {
            HashSet<int> types = new();
            types.Add(mat[y, x]);

            Deque<Pair<int, int>> q = new(), history = new();
            q.AddToBack(new(y, x));
            history.AddToBack(new(y, x));
            visited[y, x] = true;

            while (q.Count > 0) {
                Pair<int, int> cr = q.First();
                q.RemoveFromFront();
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        int nxtY = cr.St + i, nxtX = cr.Nd + j;
                        if (!visited[nxtY, nxtX] && mat[nxtY, nxtX] != -1) {
                            visited[nxtY, nxtX] = true;
                            types.Add(mat[nxtY, nxtX]);

                            q.AddToBack(new(nxtY, nxtX));
                            history.AddToBack(new(nxtY, nxtX));
                        }
                    }
                }
            }

            if (types.Count > 1) {
                foreach (var p in history)
                    mat[p.St, p.Nd] = -1;
            }
        }

        // if there're such small singularities in such low area
        // it's probably a noise
        // note: this function must only be executed after ExcludeMixedPart
        public static void ExcludeSmallPart(int[,] mat, int topMargin, int bs) {
            int h = mat.GetLength(0), w = mat.GetLength(1);
            bool[,] visited = new bool[h, w];
            MatTool<bool>.FromOuterBFSSetup(ref visited, (y, x) => { return visited[y, x] = true; });
            for (int x = 0; x < w; x++)
                visited[topMargin, x] = true;

            Iterator2D.Forward(topMargin+1, 0, h, w, (y, x) => {
                if (!visited[y, x] && mat[y, x] != -1) {
                    ExcludeSmallPartBFS(y, x, visited, mat, bs);
                }
                return true;
            });
        }

        private static void ExcludeSmallPartBFS(int y, int x, bool[,] visited, int[,] mat, int bs) {
            int threshold = bs * bs;

            Deque<Pair<int, int>> q = new(), history = new();
            q.AddToBack(new(y, x));
            history.AddToBack(new(y, x));
            visited[y, x] = true;

            while (q.Count > 0) {
                Pair<int, int> cr = q.First();
                q.RemoveFromFront();
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        int nxtY = cr.St + i, nxtX = cr.Nd + j;
                        if (!visited[nxtY, nxtX] && mat[nxtY, nxtX] != -1) {
                            visited[nxtY, nxtX] = true;

                            q.AddToBack(new(nxtY, nxtX));
                            history.AddToBack(new(nxtY, nxtX));
                        }
                    }
                }
            }

            if (history.Count <= threshold) {
                foreach (var p in history)
                    mat[p.St, p.Nd] = -1;
            }
        }
    }
}
