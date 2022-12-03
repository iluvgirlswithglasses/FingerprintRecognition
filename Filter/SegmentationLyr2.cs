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

        // returns the top and bot margin of the most significant segment
        static public Pair<int, int> GetMargin(Image<Gray, byte> src, Image<Gray, double> norm, int w) {
            
            // 0: foreground
            // 1: background before outer BFS
            // 2: background after outer BFS
            byte[,] msk = new byte[src.Height / w, src.Width / w];
            ExtractBackground(norm, w, msk);
            OuterBFS(msk);
            msk = Morphology<byte>.MonoDilation(msk, 2, (y, x) => { return y == x; });

            List<Pair<int, int>> ls = GetThinRows(msk, w);
            int mx = 0, t = 0, d = src.Height;

            for (int i = 0; i < ls.Count - 1; i++) {
                int dist = ls[i + 1].St - ls[i].Nd;
                if (dist > (norm.Height>>1) && dist > mx) {
                    mx = dist;
                    t = ls[i].Nd;
                    d = ls[i + 1].St;
                }
            }

            return new(t, d);
        }

        static private void ExtractBackground(Image<Gray, double> norm, int w, byte[,] msk) {

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

        static private void OuterBFS(byte[,] msk) {
            Deque<Pair<int, int>> q = new();
            MatTool<byte>.FromOuterBFSSetup(ref msk, (y, x) => {
                if (msk[y, x] == 1)
                    msk[y, x] = 2;
                return true;
            });
            MatTool<byte>.FromOuterBFSSetup(ref msk, (y, x) => {
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
            MatTool<byte>.Forward(ref msk, (y, x, v) => {
                if (v != 2) msk[y, x] = 0;
                return true;
            });
        }

        // res[i]: { thin row top pos, thin row down pos }
        static private List<Pair<int, int>> GetThinRows(byte[,] msk, int w) {
            // threshold for 40 blocks per row
            const int threshold = 14;

            List<Pair<int, int>> res = new();
            res.Add(new(0, 0));
            int h = msk.GetLength(0);

            for (int i = 0; i < h; i+=2) {
                int cnt = 0;
                MatTool<byte>.Forward(ref msk, i, 0, i + 2, msk.GetLength(1), (y, x, v) => {
                    if (v == 0) cnt++;
                    return true;
                });
                if (cnt <= (threshold<<1)) {
                    res.Add(new(i * w, i * w + w));
                }
            }

            res.Add(new(h, h));

            return res;
        }
    }
}
