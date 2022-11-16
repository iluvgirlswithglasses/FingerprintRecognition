using Emgu.CV;
using Emgu.CV.Structure;
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
            return res;
        }

        static public Image<Gray, double> ApplyMask(Image<Gray, double> src, bool[,] msk, int w) {
            var res = new Image<Gray, double>(src.Size);

            /** apply mask */
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    if (msk[y / w, x / w]) 
                        res[y, x] = new Gray(src[y, x].Intensity);

            return res;
        }
    }
}
