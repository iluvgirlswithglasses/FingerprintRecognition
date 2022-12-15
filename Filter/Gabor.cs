using System.Text;
using Emgu.CV;
using Emgu.CV.ML;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using FingerprintRecognition.Transform;
using static System.Math;

namespace FingerprintRecognition.Filter {

    internal class Gabor {

        static public double GetMedianFreq(double[,] freq) {
            List<double> freqList = new();
            MatTool<double>.Forward(ref freq, (y, x, val) => {
                if (val > 0) {
                    // round to nearest 0.01
                    double rnd = Round(val * 100) / 100.0;
                    freqList.Add(val);
                }
                return true;
            });

            if (freqList.Count == 0)
                return 0.1414;   // just in case

            // get the median frequency
            freqList.Sort();

            /*
            SortedDictionary<double, int> dct = new();
            foreach (var i in freqList) {
                if (!dct.ContainsKey(i))
                    dct.Add(i, 0);
                dct[i]++;
            }
            foreach (var i in dct) {
                Console.WriteLine(i.Key.ToString() + ": " + i.Value.ToString());
            }
            */

            return freqList[freqList.Count >> 1];
        }

        /** 
         * `norm`:      normalized imaged, size [ h, w ]
         * `orient`:    the orient matrix, size [ h / blocksize, w / blocksize ]
         * `freq`:      the frequency matrix, size [ h, w ]
         * */
        static public byte[,] Create(Image<Gray, double> norm, double[,] orient, double[,] freq, bool[,] msk, int imgBlockSize) {

            int h = norm.Height, w = norm.Width;
            double medianFreq = GetMedianFreq(freq);

            // Generate filters corresponding to these distinct frequencies and
            // orientations in 'ANGLE_INC' increments.
            double sigma = 1 / medianFreq * 0.65;
            int blockSize = (int)Round(3.0 * sigma);
            int filterSize = blockSize << 1 | 1;

            double[,] sigmaMaskX = new double[filterSize, filterSize];
            double[,] sigmaMaskY = new double[filterSize, filterSize];
            MatTool<double>.Forward(ref sigmaMaskX, (y, x, val) => {
                sigmaMaskX[y, x] = (x - blockSize);
                sigmaMaskY[y, x] = (y - blockSize);
                return true;
            });

            // gabor filter equation:
            //      reffilter = exp(refa) * cos(refb)
            //      refa = - ( sigmaMaskX/(sigma**2) + sigmaMaskY/(sigma**2) )
            //      refb = 2 * PI * median * sigmaMaskX
            double sigmaSqr = sigma * sigma;
            var refa = new Image<Gray, double>(filterSize, filterSize);
            var refb = refa.Copy();
            ImgTool<double>.Forward(ref refa, (y, x, val) => {
                refa[y, x] = new Gray(
                   (-(sigmaMaskX[y, x] * sigmaMaskX[y, x]) / sigmaSqr - (sigmaMaskY[y, x] * sigmaMaskY[y, x]) / sigmaSqr) / 2
                );
                refb[y, x] = new Gray(Cos(2 * PI * medianFreq * sigmaMaskX[y, x]));
                return true;
            });
            CvInvoke.Exp(refa, refa);

            double[,] refFilter = new double[filterSize, filterSize];
            MatTool<double>.Forward(ref refFilter, (y, x, val) => {
                refFilter[y, x] = refa[y, x].Intensity * refb[y, x].Intensity;
                return true;
            });

            // convolution & returns
            byte[,] res = new byte[h, w];
            QuickFilt(res, norm, orient, freq, refFilter, msk, blockSize, filterSize, imgBlockSize);
            return res;
        }

        static private void QuickFilt(byte[,] res, Image<Gray, double> norm, double[,] orient, double[,] freq, double[,] refFilter, bool[,] msk, int blockSize, int filterSize, int bs) {

            double angleInc = PI * 3 / 180;
            List<double> acceptedAngles = new();
            for (double i = -PI/2; i <= PI/2; i += angleInc)
                acceptedAngles.Add(i);
            List<double> compressed = CompressAngle(orient, acceptedAngles);
            //
            List<double[,]> filters = new();
            foreach (double angle in compressed)
                filters.Add(AffineRotation<double>.KeepSizeCreate(refFilter, angle));

            /** 
             * @ note for improvement:
             *      this stage might be optimized using FFT
             * */
            //
            Iterator2D.Forward(blockSize, blockSize, norm.Height - blockSize, norm.Width - blockSize, (y, x) => {
                if (!msk[y, x])
                    return false;

                // trials and errors
                double angle = PI / 2 - orient[y / bs, x / bs];
                int angleInd = LowerBound(compressed, angle);
                double val = 0;

                for (int r = 0; r < filterSize; r++) {
                    for (int c = 0; c < filterSize; c++) {
                        int localY = y - blockSize + r;
                        int localX = x - blockSize + c;

                        val += norm[localY, localX].Intensity * filters[angleInd][r, c];
                    }
                }

                if (val < 0) res[y, x] = 255;
                // else res[y, x] = 0;

                return true;
            });
        }

        /** @ tools */
        static private List<double> CompressAngle(double[,] orient, List<double> acceptedAngles) {
            SortedSet<double> compressed = new();
            MatTool<double>.Forward(ref orient, (y, x, v) => {
                compressed.Add(
                    acceptedAngles[LowerBound(acceptedAngles, PI/2 - v)]
                );
                return true;
            });
            return new List<double>(compressed);
        }

        static private int LowerBound(List<double> ls, double x) {
            int l = 0, r = ls.Count - 1;    // there's no ls.end() returned
            while (l < r) {
                int m = (l + r + 0) >> 1;
                if (ls[m] >= x)
                    r = m;
                else
                    l = m + 1;
            }
            return r;
        }
    }
}
