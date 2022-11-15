using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter {
    internal class Gabor {
        static public bool[,] Create(Image<Gray, double> norm, double[,] orient, double[,] freq) {
            const int ANGLE_INC = 3;

            int h = norm.Height, w = norm.Width;
            bool[,] res = new bool[h, w];

            List<double> freqList = new();
            MatTool<double>.Forward(ref freq, (y, x, val) => {
                if (val > 0) {
                    // round to nearest 0.01
                    double rnd = Round(val * 100) / 100.0;
                    freqList.Add(val);
                }
                return true;
            });

            // get the median frequency
            freqList.Sort();
            double medianFreq = freqList.ToList()[freqList.Count >> 1];

            Console.WriteLine("medianFreq: {0}", medianFreq);

            // Generate filters corresponding to these distinct frequencies and
            // orientations in 'ANGLE_INC' increments.
            double sigma = 1 / medianFreq * 0.65;
            int blockSize = (int)Round(3.0 * sigma);
            double[,] sigmaMaskX = new double[blockSize<<1|1, blockSize<<1|1];
            double[,] sigmaMaskY = new double[blockSize<<1|1, blockSize<<1|1];
            MatTool<double>.Forward(ref sigmaMaskX, (y, x, val) => {
                sigmaMaskX[y, x] = (x - blockSize) * (x - blockSize);
                sigmaMaskY[y, x] = (y - blockSize) * (y - blockSize);
                return true;
            });

            // gabor filter equation:
            //      reffilter = exp(refa) * cos(refb)
            //      refa = - ( sigmaMaskX/(sigma**2) + sigmaMaskY/(sigma**2) )
            //      refb = 2 * PI * median * sigmaMaskX
            double sigmaSqr = sigma * sigma;
            var refa = new Image<Gray, double>(sigmaMaskX.GetLength(1), sigmaMaskX.GetLength(0));
            var refb = refa.Copy();
            ImgTool<double>.Forward(ref refa, (y, x, val) => {
                refa[y, x] = new Gray(- sigmaMaskX[y, x]/sigmaSqr - sigmaMaskY[y, x]/sigmaSqr);
                refb[y, x] = new Gray(Cos(2 * PI * medianFreq * sigmaMaskX[y, x]));
                return true;
            });
            CvInvoke.Exp(refa, refa);

            double[,] reffilter = new double[refa.Height, refa.Width];
            MatTool<double>.Forward(ref reffilter, (y, x, val) => {
                reffilter[y, x] = refa[y, x].Intensity * refb[y, x].Intensity;
                return true;
            });

            return res;
        }
    }
}
