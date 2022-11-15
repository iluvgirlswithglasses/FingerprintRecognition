using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.Convolution {

    internal class KernelApplier {

        public static Image<Gray, double> Apply(Image<Gray, double> src, int[,] kernel) {
            var res = new Image<Gray, double>(src.Size);
            for (int y = 1; y < res.Height - 1; y++)
                for (int x = 1; x < res.Width - 1; x++)
                    res[y, x] = new Gray(Calc(ref src, kernel, y, x));
            return res;
        }

        public static Image<Gray, double> Apply(Image<Gray, double> src, int[,] kernelY, int[,] kernelX) {
            // x-gradiant & y-gradiant matrix
            Image<Gray, double>
                res = new Image<Gray, double>(src.Size),
                gy = Apply(src, kernelY),
                gx = Apply(src, kernelX);

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray(
                        Math.Max(gy[y, x].Intensity, gx[y, x].Intensity)
                    );

            return res;
        }

        private static double Calc(ref Image<Gray, double> src, int[,] kernel, int y, int x) {
            double res = 0;
            for (int i = 0; i <= 2; i++)
                for (int j = 0; j <= 2; j++)
                    res += src[y-1+i, x-1+j].Intensity * kernel[i, j];
            return res;
        }

        private static double Sqr(double x) {
            return x * x;
        }
    }
}
