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

            HashSet<double> uniqueFreq = new HashSet<double>();
            MatTool<double>.Forward(ref freq, (y, x, val) => {
                if (val > 0) {
                    // round to nearest 0.01
                    // to reduce unique numbers
                    double rnd = Round(val * 100) / 100.0;
                    uniqueFreq.Add(rnd);
                }
                return true;
            });

            // Generate filters corresponding to these distinct frequencies and
            // orientations in 'ANGLE_INC' increments.
            double[] sigma = new double[uniqueFreq.Count];
            double sigmaMax = 0.0;
            int _counter = 0;
            foreach (double i in uniqueFreq) {
                sigma[_counter] = 1 / i * 0.65;
                sigmaMax = Max(sigmaMax, sigma[_counter]);
                _counter++;
            }
            int blockSize = (int)Round(3.0 * sigmaMax);
            double[,] sigmaMaskX = new double[blockSize<<1|1, blockSize<<1|1];
            double[,] sigmaMaskY = new double[blockSize<<1|1, blockSize<<1|1];
            MatTool<double>.Forward(ref sigmaMaskX, (y, x, val) => {
                sigmaMaskX[y, x] = (x - blockSize) * (x - blockSize);
                sigmaMaskY[y, x] = (y - blockSize) * (y - blockSize);
                return true;
            });

            

            // gabor filter equation
            // reffilter = refa * refb
            // refa = - ( sigmaMaskX/(sigma**2) + sigmaMaskY/(sigma**2) )
            // refb = cos( 2 * PI * uniqueFreq[0] * sigmaMaskX )
            

            return res;
        }
    }
}
