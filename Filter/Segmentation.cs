using Emgu.CV;
using Emgu.CV.Structure;
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
        static public Image<Gray, double> Create(ref Image<Gray, double> src, int w)
        {
            var res = new Image<Gray, double>(src.Size);

            /** get useful segments */
            var msk = new bool[res.Height, res.Height];
            var threshold = 0.2 * Tool.MatTool<double>.Std(ref src);

            for (int y = 0; y < src.Height; y+=w)
            {
                for (int x = 0; x < src.Width; x+=w)
                {
                    double std = Tool.MatTool<double>.Std(
                        ref src, y, x, Min(y + w, src.Height), Min(x + w, src.Width)
                    );
                    if (std < threshold)
                        continue;

                    // found a useful block
                    for (int i = 0; i < Min(w, src.Height - y); i++)
                        for (int j = 0; j < Min(w, src.Width - x); j++)
                            msk[y + i, x + j] = true;
                }
            }

            /** apply mask */
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    if (msk[y, x]) 
                        res[y, x] = new Gray(src[y, x].Intensity);

            return res;
        }
    }
}
