using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter
{
    internal class Normalization
    {
        static public Image<Gray, double> Normalize(ref Image<Gray, byte> src, double m0, double v0)
        {
            var res = new Image<Gray, double>(src.Size);
            double m = src.GetAverage().Intensity, 
                   v = MatTool<byte>.Std(ref src);

            MatTool<byte>.Forward(ref src, (y, x, val) => {
                res[y, x] = new Gray(NormalizePixel(m0, v0, val, m, v));
                return true;
            });

            return res;
        }

        static private double NormalizePixel(double m0, double v0, double px, double m, double v)
        {
            double coeff = Sqrt( v0 * ((px - m) * (px - m))) / v;
            if (px > m)
                return m0 + coeff;
            return m0 - coeff;
        }

        static public Image<Gray, double> AllignAvg(ref Image<Gray, double> norm)
        {
            var res = new Image<Gray, double>(norm.Size);

            double avg = norm.GetAverage().Intensity;
            double std = MatTool<double>.Std(ref norm);

            MatTool<double>.Forward(ref norm, (y, x, v) => {
                res[y, x] = new Gray((v - avg) / std);
                return true;
            });

            return res;
        }

        static public Image<Gray, double> AllignWithMask(ref Image<Gray, double> norm, ref bool[,] msk, int w)
        {
            var res = new Image<Gray, double>(norm.Size);

            int n = 0;
            double sum = 0.0, avg = 0.0, std = 0.0;

            for (int y = 0; y < msk.GetLength(0); y++)
                for (int x = 0; x < msk.GetLength(1); x++)
                    if (!msk[y, x])
                    {
                        sum += MatTool<double>.Sum(ref norm, y * w, x * w, y * w + w, x * w + w);
                        n += w * w;
                    }

            avg = sum / n;

            for (int y = 0; y < msk.GetLength(0); y++)
                for (int x = 0; x < msk.GetLength(1); x++)
                    if (!msk[y, x])
                        std += MatTool<double>.Sum(
                            ref norm, y*w, x*w, y*w + w, x*w + w, (x) => { return (x - avg) * (x - avg); }
                        );

            std = Sqrt(std / n);

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray((norm[y, x].Intensity - avg) / std);
            
            return res;
        }
    }
}
