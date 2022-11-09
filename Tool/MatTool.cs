using Emgu.CV;
using Emgu.CV.Structure;
using static System.Math;

namespace FingerprintRecognition.Tool
{
    internal class MatTool
    {
        /** @ tools for byte images */
        static public double Std(ref Image<Gray, byte> src)
        {
            /*
            The standard deviation is the square root of the average of the 
            squared deviations from the mean, i.e., std = sqrt(mean(x)), 
            where x = abs(a - a.mean())**2.
            */
            double avg = src.GetAverage().Intensity;
            double res = 0;
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    res += sqr(src[y, x].Intensity - avg);

            return Sqrt(res / (src.Height * src.Width));
        }

        /** @ tools for double images */
        static public double Max(ref Image<Gray, double> src)
        {
            double mx = -1e18;
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    mx = Math.Max(mx, src[y, x].Intensity);
            return mx;
        }

        /** @ calculators */
        static private double sqr(double x)
        {
            return x * x;
        }
    }
}
