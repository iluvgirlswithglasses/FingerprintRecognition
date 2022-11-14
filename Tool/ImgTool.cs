using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.Tool
{
    internal class ImgTool<TDepth> where TDepth : new()
    {
        /** @ the core */
        static public void Forward(ref Image<Gray, TDepth> src, Func<int, int, double, bool> f)
        {
            Forward(ref src, 0, 0, src.Height, src.Width, f);
        }

        static public void Forward(ref Image<Gray, TDepth> src, int t, int l, int d, int r, Func<int, int, double, bool> f) 
        {
            t = Math.Max(0, t);
            l = Math.Max(0, l);
            d = Math.Min(src.Height, d);
            r = Math.Min(src.Width, r);
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    f(y, x, src[y, x].Intensity);
        }

        /** @ std shortcuts */
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

            return Math.Sqrt(res / (src.Height * src.Width));
        }

        static public double Std(ref Image<Gray, TDepth> src, int t, int l, int d, int r)
        {
            /*
            The standard deviation is the square root of the average of the 
            squared deviations from the mean, i.e., std = sqrt(mean(x)), 
            where x = abs(a - a.mean())**2.
            */
            double avg = Sum(ref src, t, l, d, r);
            double res = 0;

            avg /= (Math.Min(src.Height, d) - t) * (Math.Min(src.Width, r) - l);

            Forward(ref src, t, l, d, r, (y, x, v) => {
                res += Sqr(v - avg);
                return true;
            });

            return Math.Sqrt(
                res / ((d - t) * (r - l))
            );
        }

        /** @ sum shortcuts */
        static public double Sum(ref Image<Gray, TDepth> src, Func<double, double> f)
        {
            return Sum(ref src, 0, 0, src.Height, src.Width, f);
        }

        static public double Sum(ref Image<Gray, TDepth> src, int t, int l, int d, int r)
        {
            return Sum(ref src, t, l, d, r, (x) => { return x; });
        }

        static public double Sum(ref Image<Gray, TDepth> src, int t, int l, int d, int r, Func<double, double> f)
        {
            double res = 0.0;
            Forward(ref src, t, l, d, r, (y, x, v) => {
                res += f(v);
                return true;
            });
            return res;
        }

        /** @ min-max shortcuts */
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

        /** @ calculators */
        static public double Sqr(double x)
        {
            return x * x;
        }
    }
}
