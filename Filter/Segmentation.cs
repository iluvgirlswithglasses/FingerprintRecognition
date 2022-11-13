using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.BinaryEffect;
using static System.Math;

namespace FingerprintRecognition.Filter
{
    internal class Segmentation
    {
        /** 
         * extracts the fingerprint part
         * 
         * src: normalized image
         * w:   kernel size
         */
        static public bool[,] CreateMask(ref Image<Gray, double> src, int w)
        {
            // msk[y, x] manages the block[ y*w : (y+1)*w ][ x*w : (x+1)*w ]
            var msk = new bool[
                (int)Ceiling(Convert.ToDouble(src.Height) / w),
                (int)Ceiling(Convert.ToDouble(src.Width) / w)
            ];
            var threshold = 0.2 * Tool.ImgTool<double>.Std(ref src);

            for (int y = 0; y < msk.GetLength(0); y++)
            {
                for (int x = 0; x < msk.GetLength(1); x++)
                {
                    double std = Tool.ImgTool<double>.Std(
                        ref src, y * w, x * w, Min(y * w + w, src.Height), Min(x * w + w, src.Width)
                    );
                    msk[y, x] = std > threshold;
                }
            }

            // apply Morphological Opening & Closing to the mask
            // https://docs.opencv.org/4.x/d9/d61/tutorial_py_morphological_ops.html
            Morphology.Open(ref msk);
            Morphology.Close(ref msk);

            return msk;
        }

        static public Image<Gray, double> ApplyMask(ref Image<Gray, double> src, ref bool[,] msk, int w)
        {
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
