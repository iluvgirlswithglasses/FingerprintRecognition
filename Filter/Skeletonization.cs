using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Tool;

// this skel solution has a freaking bug
// imma make another skel sol based on this idea:
// https://felix.abecassis.me/2011/09/opencv-morphological-skeleton/

namespace FingerprintRecognition.Filter {

    public class Skeletonization {
        bool[,] Img;
        int height, width;

        /*
        adj cells look like this:

        0 1 2
        7   3
        6 5 4
        */
        readonly int[] Y = { -1, -1, -1, 0, 1, 1, 1, 0 };
        readonly int[] X = { -1, 0, 1, 1, 1, 0, -1, -1 };

        public Skeletonization(bool[,] src) {
            Img = src;
            height = Img.GetLength(0);
            width = Img.GetLength(1);
        }

        public void Apply() {
            Deque<KeyValuePair<int, int>> queue = new();
            // get the outline first
            Iterator2D.Forward(2, 2, height - 2, width - 2, (y, x) => {
                if (!Img[y, x]) Bleed(ref queue, y, x);
                return true;
            });
            // bfs
            while (queue.Count > 0) {
                var p = queue.First();
                queue.RemoveFromFront();
                // test
                if (CanRemove(p.Key, p.Value)) {
                    Img[p.Key, p.Value] = false;
                    Bleed(ref queue, p.Key, p.Value);
                }
            }
        }

        private void Bleed(ref Deque<KeyValuePair<int, int>> queue, int y, int x) {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    if (Img[y + i, x + j]) queue.AddToBack(new(y + i, x + j));
        }

        private bool CanRemove(int y, int x) {
            int adj = GetAdj(y, x), shf = GetShift(y, x);

            return 
                (2 <= adj && adj <= 6) &&
                (shf == 1) &&
                (!GetRelativeCell(y, x, 0) || !GetRelativeCell(y, x, 2) || !GetRelativeCell(y, x, 6) || GetShift(y + Y[0], x + X[0]) != 1) &&
                (!GetRelativeCell(y, x, 0) || !GetRelativeCell(y, x, 2) || !GetRelativeCell(y, x, 4) || GetShift(y + Y[2], x + X[2]) != 1);
        }

        private bool GetRelativeCell(int y, int x, int i) {
            return Img[y + Y[i], x + X[i]];
        }

        private int GetAdj(int y, int x) {
            int adj = 0;
            for (int i = 0; i < 8; i++)
                if (Img[y + Y[i], x + X[i]])
                    adj++;
            // the ridge of a fingerprint only split from 1 to 2 path 
            return adj;
        }

        private int GetShift(int y, int x) {
            int shf = 0;
            for (int i = 0; i < 8; i++) {
                if (!Img[y + Y[i], x + X[i]] && Img[y + Y[AdjMod(i + 1)], x + X[AdjMod(i + 1)]])
                    shf++;
            }
            return shf;
        }

        static private int AdjMod(int i) {
            return i >= 8 ? i - 8 : i;
        }

        /** 
         * @ remove short ridges
         * 
         * note for improvements:
         *      
         *      instead of just removing ridges that are too short
         *      we can remove branches of a ridge that are too short
         *      by encoding a ridge into a tree and 
         *      applying the solution I composed in my pb "Cirno's Company"
         *      
         * */
        static public void RemoveShortRidges(bool[,] ske, int threshold) {
            int h = ske.GetLength(0), w = ske.GetLength(1);
            bool[,] visited = new bool[h, w];

            // make sure there's no going out of image
            for (int y = 0; y < h; y++)
                visited[y, 0] = visited[y, w - 1] = true;
            for (int x = 0; x < w; x++)
                visited[0, x] = visited[h - 1, x] = true;

            Iterator2D.Forward(1, 1, h - 1, w - 1, (y, x) => {
                if (ske[y, x] && !visited[y, x])
                    RemoveShortRidgesBFS(ske, visited, y, x, threshold);
                return true;
            });
        }

        static private void RemoveShortRidgesBFS(bool[,] ske, bool[,] visited, int y, int x, int threshold) {
            Deque<Pair<int, int>> q = new();
            q.AddToBack(new(y, x));
            visited[y, x] = true;

            Deque<Pair<int, int>> history = new();
            history.AddToBack(new(y, x));

            while (q.Count > 0) {
                Pair<int, int> cr = q.First();
                q.RemoveFromFront();

                for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++) {
                    int nxtY = cr.St + i, nxtX = cr.Nd + j;
                    if (!visited[nxtY, nxtX] && ske[nxtY, nxtX]) {
                        visited[nxtY, nxtX] = true;
                        q.AddToBack(new(nxtY, nxtX));

                        history.AddToBack(new(nxtY, nxtX));
                    }
                }
            }

            if (history.Count < threshold) {
                foreach (var i in history)
                    ske[i.St, i.Nd] = false;
            }
        }
    }
}
