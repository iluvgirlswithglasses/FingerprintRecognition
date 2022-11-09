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
            var imgVariance = src.Copy();

            var threshold = 0.2 * Tool.MatTool<double>.Std(ref src);

            for (int y = 0; y < imgVariance.Height; y+=w)
            {
                for (int x = 0; x < imgVariance.Width; x+=w)
                {
                    double std = Tool.MatTool<double>.Std(
                        ref src, y, x, Min(y + w, src.Height), Min(x + w, src.Width)
                    );
                    if (std < threshold) continue;

                    // found a useful block
                    for (int i = 0; i < Min(w, imgVariance.Height - y); i++)
                        for (int j = 0; j < Min(w, imgVariance.Width - x); j++)
                            imgVariance[y + i, x + j] = new Gray(std);
                }
            }

            return res;
        }
    }
}
