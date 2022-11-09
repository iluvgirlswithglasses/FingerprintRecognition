using Emgu.CV;
using Emgu.CV.Structure;
using static System.Math;

namespace FingerprintRecognition.Filter
{
    internal class Normalization
    {
        static public Image<Gray, double> Normalize(ref Image<Gray, byte> src, double m0, double v0)
        {
            var res = new Image<Gray, double>(src.Size);
            double m = src.GetAverage().Intensity, 
                   v = Tool.MatTool<byte>.Std(ref src);

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray(NormalizePixel(m0, v0, src[y, x].Intensity, m, v));

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
            double std = Tool.MatTool<double>.Std(ref norm);

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray((norm[y, x].Intensity - avg) / std);

            return res;
        }
    }
}
