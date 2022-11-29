using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter
{

    internal class Segmentation {

        /** 
         * extracts the fingerprint part
         * 
         * src: normalized image
         * w:   kernel size
         */
        static public bool[,] CreateMask(Image<Gray, double> src, int w) {
            var res = new bool[src.Height, src.Width];

            Image<Gray, double> msk = new(src.Size);
            var threshold = 0.2 * ImgTool<double>.Std(ref src);

            for (int y = 0; y < src.Height; y+=w) {
                for (int x = 0; x < src.Width; x+=w) {
                    double std = ImgTool<double>.Std(
                        ref src, y, x, y + w, x + w
                    );
                    if (std >= threshold) {
                        ImgTool<double>.Forward(ref msk, y, x, y + w, x + w, (r, c, val) => {
                            msk[r, c] = new Gray(255);
                            return true;
                        });
                    }
                }
            }

            // apply Morphological Opening & Closing to the mask
            // https://docs.opencv.org/4.x/d9/d61/tutorial_py_morphological_ops.html
            var kernel = CvInvoke.GetStructuringElement(
                Emgu.CV.CvEnum.ElementShape.Ellipse, 
                new System.Drawing.Size(w << 1, w << 1), 
                new System.Drawing.Point(-1, -1)
            );
            var center = new System.Drawing.Point(-1, -1);
            var border = Emgu.CV.CvEnum.BorderType.Default;

            CvInvoke.MorphologyEx(msk, msk, Emgu.CV.CvEnum.MorphOp.Open, kernel, center, 1, border, new Gray(0).MCvScalar);
            CvInvoke.MorphologyEx(msk, msk, Emgu.CV.CvEnum.MorphOp.Close, kernel, center, 1, border, new Gray(0).MCvScalar);

            // cast to bool[]
            ImgTool<double>.Forward(ref msk, (y, x, v) => {
                res[y, x] = v > 0;
                return true;
            });

            // trim the mask
            BFSTrim(res, 5);

            return res;
        }

        static public Image<Gray, double> ApplyMask(Image<Gray, double> src, bool[,] msk) {
            var res = new Image<Gray, double>(src.Size);

            /** apply mask */
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    if (msk[y, x]) 
                        res[y, x] = new Gray(src[y, x].Intensity);

            return res;
        }

        static private void BFSTrim(bool[,] msk, int depth) {
            int h = msk.GetLength(0), w = msk.GetLength(1);

            Deque<Pair<int, int>> q = new();
            Deque<int> d = new();
            
            bool[,] visited = new bool[h, w];
            // make sure there's no going out of image
            for (int y = 0; y < h; y++) {
                visited[y, 0] = visited[y, w - 1] = true;
                msk[y, 0] = msk[y, w - 1] = false;
            }
            for (int x = 0; x < w; x++) {
                visited[0, x] = visited[h - 1, x] = true;
                msk[0, x] = msk[h - 1, x] = false;
            }
                
            // getting in from the outside
            for (int y = 1; y < h - 1; y++) {
                visited[y, 1] = visited[y, w - 2] = true;
                q.AddToBack(new(y, 1));
                q.AddToBack(new(y, w - 2));
                d.AddToBack(0);
                d.AddToBack(0);
            }
            for (int x = 1; x < w - 1; x++) {
                visited[1, x] = visited[h - 2, x] = true;
                q.AddToBack(new(1, x));
                q.AddToBack(new(h - 2, x));
                d.AddToBack(0);
                d.AddToBack(0);
            }

            while (q.Count > 0) {
                Pair<int, int> cr = q.First();
                int crDepth = d.First();
                q.RemoveFromFront();
                d.RemoveFromFront(); 
                msk[cr.St, cr.Nd] = false;

                //
                if (crDepth < depth)
                for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++) {
                    int nxtY = cr.St + i, nxtX = cr.Nd + j, nxtD = crDepth;
                    if (!visited[nxtY, nxtX]) {
                        visited[nxtY, nxtX] = true;
                        if (msk[nxtY, nxtX]) {
                            nxtD++;
                            msk[nxtY, nxtX] = false;
                        }
                        //
                        q.AddToBack(new(nxtY, nxtX));
                        d.AddToBack(nxtD);
                    }
                }
            }
        }
    }
}
