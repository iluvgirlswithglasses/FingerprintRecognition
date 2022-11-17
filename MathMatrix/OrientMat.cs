using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Convolution;
using FingerprintRecognition.Tool;
using System.Drawing;
using static System.Math;

namespace FingerprintRecognition.MathMatrix {

    internal class OrientMat {

        static public double[,] Create(Image<Gray, double> norm, int w) {
            // res[y, x] controls the range norm[ y*w : (y+1)*w ][ x*w : (x+1)*w ]
            var res = new double[
                (int)Ceiling(Convert.ToDouble(norm.Height) / w),
                (int)Ceiling(Convert.ToDouble(norm.Width) / w)
            ];

            // x-gradiant & y-gradiant matrix
            Image<Gray, double> 
                gy = KernelApplier.Apply(norm, SobelOperator.Y_KERNEL),
                gx = KernelApplier.Apply(norm, SobelOperator.X_KERNEL);

            for (int y0 = 0; y0 < norm.Height; y0+=w)
            for (int x0 = 0; x0 < norm.Width; x0+=w) {
                double a = 0.0, b = 0.0;

                for (int y = y0; y < Min(y0+w, norm.Height); y++)
                for (int x = x0; x < Min(x0+w, norm.Width); x++) {
                    // this simplification ensures a simple orientation image
                    double yAngle = gy[y, x].Intensity,
                           xAngle = gx[y, x].Intensity;
                    a += 2.0 * yAngle * xAngle;
                    b += xAngle*xAngle - yAngle*yAngle;
                }

                if (a != 0.0 && b != 0.0) 
                    res[y0 / w, x0 / w] = (PI + Atan2(a, b)) / 2;
            }

            return res;
        }

        static public Image<Gray, byte> Visualize(Image<Gray, double> segment, bool[,] msk, double[,] orient, int s) {
            Image<Gray, byte> res = new(segment.Size);
            
            int h = orient.GetLength(0), w = orient.GetLength(1);
            
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (msk[y, x] && orient[y, x] != 0) {
                        double alpha = Tan(orient[y, x]);
                        int sX = x * s, sY = y * s;
                        int eX = x * s + s, eY = y * s + (int) alpha * s;

                        eY = Max(eY, 0);
                        eY = Max(eY, sY - s);
                        eY = Min(eY, sY + s);

                        CvInvoke.Line(res, new Point(sX, sY), new Point(eX, eY), new Gray(255.0).MCvScalar);
                    }
                }
            }

            return res;
        }
    }
}
