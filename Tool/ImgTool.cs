using Emgu.CV;
using Emgu.CV.Structure;
using static System.Math;

namespace FingerprintRecognition.Tool
{
    internal class ImgTool<TDepth> where TDepth : new()
    {
        /** @ the core */
        static public void Forward(ref Image<Gray, TDepth> src, Func<int, int, double, bool> f)
        {
            Forward(ref src, 0, 0, src.Height, src.Width, (y, x, v) => { return true; }, f);
        }

        static public void Forward(ref Image<Gray, TDepth> src, int t, int l, int d, int r, Func<int, int, double, bool> f)
        {
            Forward(ref src, t, l, d, r, (y, x, v) => { return true; }, f);
        }

        static public void Forward(ref Image<Gray, TDepth> src, Func<int, int, double, bool> g, Func<int, int, double, bool> f)
        {
            Forward(ref src, 0, 0, src.Height, src.Width, g, f);
        }

        static public void Forward(
            ref Image<Gray, TDepth> src,
            int t, int l, int d, int r,
            Func<int, int, double, bool> g, 
            Func<int, int, double, bool> f
        ) {
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    // if g() then execute f()
                    if (g(y, x, src[y, x].Intensity))
                        f(y, x, src[y, x].Intensity);
        }

        /** @ shortcuts */
        static public double Std(ref Image<Gray, TDepth> src)
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
                    res += Sqr(src[y, x].Intensity - avg);

            return Sqrt(res / (src.Height * src.Width));
        }

        static public double Sum(ref Image<Gray, TDepth> src, Func<double, double> f)
        {
            return Sum(ref src, 0, 0, src.Height, src.Width, f);
        }

        static public double Max(ref Image<Gray, TDepth> src)
        {
            double mx = Double.MinValue;
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    mx = Math.Max(mx, src[y, x].Intensity);
            return mx;
        }

        static public double Min(ref Image<Gray, TDepth> src)
        {
            double mn = Double.MaxValue;
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    mn = Math.Min(mn, src[y, x].Intensity);
            return mn;
        }

        /** @ extensions */
        static public double Std(ref Image<Gray, TDepth> src, int t, int l, int d, int r)
        {
            /*
            The standard deviation is the square root of the average of the 
            squared deviations from the mean, i.e., std = sqrt(mean(x)), 
            where x = abs(a - a.mean())**2.
            */
            double avg = Sum(ref src, t, l, d, r) / ((d - t) * (r - l));
            double res = 0;
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    res += Sqr(src[y, x].Intensity - avg);

            return Sqrt(
                res / ((d - t) * (r - l))
            );
        }

        static public double Sum(ref Image<Gray, TDepth> src, int t, int l, int d, int r, Func<double, double> f)
        {
            double res = 0.0;
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    res += f(src[y, x].Intensity);
            return res;
        }

        static public double Sum(ref Image<Gray, TDepth> src, int t, int l, int d, int r)
        {
            return Sum(ref src, t, l, d, r, (x) => { return x; });
        }

        /** @ calculators */
        static public double Sqr(double x)
        {
            return x * x;
        }
    }
}
