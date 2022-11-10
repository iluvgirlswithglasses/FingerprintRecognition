using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Convolution;
using static System.Math;

namespace FingerprintRecognition.MathMatrix
{
    internal class AngleMat
    {
        static public double[,] Create(ref Image<Gray, double> norm, int w=16)
        {
            // res[y, x] controls the range norm[ y*w : (y+1)*w ][ x*w : (x+1)*w ]
            var res = new double[
                (int)Ceiling(Convert.ToDouble(norm.Height) / w),
                (int)Ceiling(Convert.ToDouble(norm.Height) / w)
            ];

            // x-gradiant & y-gradiant matrix
            Image<Gray, double> 
                gy = KernelApplier.Apply(ref norm, SobelOperator.Y_KERNEL),
                gx = KernelApplier.Apply(ref norm, SobelOperator.X_KERNEL);

            for (int y0 = 0; y0 < norm.Height; y0+=w)
            for (int x0 = 0; x0 < norm.Width; x0+=w)
            {
                double a = 0.0, b = 0.0;
                for (int y = y0; y < Min(y0+w, norm.Height); y++)
                for (int x = x0; x < Min(x0+w, norm.Width); x++)
                {
                    double yAngle = Round(gy[y, x].Intensity),
                           xAngle = Round(gx[y, x].Intensity);
                    a += 2.0 * yAngle * xAngle;
                    b += xAngle*xAngle - yAngle*yAngle;
                }

                if (a != 0.0 || b != 0.0) 
                    res[y0 / w, x0 / w] = (PI + Atan2(a, b)) / 2;
            }

            return res;
        }
    }
}
