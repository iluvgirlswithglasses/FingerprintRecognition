using Emgu.CV.Structure;
using Emgu.CV;
using FingerprintRecognition.Tool;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Transform;

namespace FingerprintRecognition.Filter {

    /**
     * the input fingerprint might have been serperated into multiple segment
     * this mask will keep the most significant segment only
     * 
     * the kernel used in this mask should be much smaller than the other one
     * */

    internal class SegmentationLyr2 {

        static public bool[,] Create(Image<Gray, byte> src, Image<Gray, double> norm, int w) {
            
            // 0: foreground
            // 1: background before outer BFS
            // 2: background after outer BFS
            int[,] msk = new int[src.Height / w, src.Width / w];
            ExtractBackground(norm, w, msk);
            OuterBFS(msk);
            msk = Morphology<int>.MonoDilation(msk, 2, (y, x) => { return y == x; });

            bool[,] res = new bool[src.Height, src.Width];
            MatTool<int>.Forward(ref msk, (i, j, v) => {
                if (v == 2)
                    MatTool<bool>.Forward(ref res, i * w, j * w, i * w + w, j * w + w, (y, x, v) => {
                        res[y, x] = true;
                        return true;
                    });
                return true;
            });

            return res;
        }

        static private void ExtractBackground(Image<Gray, double> norm, int w, int[,] msk) {

            var threshold = 0.2 * ImgTool<double>.Std(ref norm);

            for (int i = 0; i < msk.GetLength(0); i++) {
                for (int j = 0; j < msk.GetLength(1); j++) {
                    int y = i * w, x = j * w;

                    double std = ImgTool<double>.Std(
                        ref norm, y, x, y + w, x + w
                    );
                    if (std < threshold)
                        msk[i, j] = 1;
                }
            }
        }

        static private void OuterBFS(int[,] msk) {
            Deque<Pair<int, int>> q = new();
            MatTool<int>.FromOuterBFSSetup(ref msk, (y, x) => {
                if (msk[y, x] == 1)
                    msk[y, x] = 2;
                return true;
            });
            MatTool<int>.FromOuterBFSSetup(ref msk, (y, x) => {
                if (msk[y, x] == 1) {
                    msk[y, x] = 2;
                    q.AddToBack(new(y, x));
                }
                return true;
            }, 1);
            while (q.Count > 0) {
                Pair<int, int> cr = q.First();
                q.RemoveFromFront();
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        int nxtY = cr.St + i, nxtX = cr.Nd + j;
                        if (msk[nxtY, nxtX] == 1) {
                            q.AddToBack(new(nxtY, nxtX));
                            msk[nxtY, nxtX] = 2;
                        }
                    }
                }
            }
            // mark anything other than outer background as "foreground"
            MatTool<int>.Forward(ref msk, (y, x, v) => {
                if (v != 2) msk[y, x] = 0;
                return true;
            });
        }
    }
}
