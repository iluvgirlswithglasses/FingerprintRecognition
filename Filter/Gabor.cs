using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using FingerprintRecognition.Transform;
using static System.Math;

namespace FingerprintRecognition.Filter {

    internal class Gabor {

        /** 
         * `norm`:      normalized imaged
         * `orient`:    the orient matrix, size [ h / blocksize, w / blocksize ]
         * `freq`:      the frequency matrix, size [ h / blocksize, w / blocksize ]
         * */
        static public double[,] Create(Image<Gray, double> norm, double[,] orient, double[,] freq, int imgBlockSize) {

            const int ANGLE_INC = 3;
            int h = norm.Height, w = norm.Width;

            double[,] res = new double[h, w];
            double medianFreq = GetMedianFreq(freq);

            Console.WriteLine("medianFreq: {0}", medianFreq);

            // Generate filters corresponding to these distinct frequencies and
            // orientations in 'ANGLE_INC' increments.
            double sigma = 1 / medianFreq * 0.65;
            int blockSize = (int)Round(3.0 * sigma);
            int filterSize = blockSize << 1 | 1;

            double[,] sigmaMaskX = new double[filterSize, filterSize];
            double[,] sigmaMaskY = new double[filterSize, filterSize];
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
            var refa = new Image<Gray, double>(filterSize, filterSize);
            var refb = refa.Copy();
            ImgTool<double>.Forward(ref refa, (y, x, val) => {
                refa[y, x] = new Gray(- sigmaMaskX[y, x]/sigmaSqr - sigmaMaskY[y, x]/sigmaSqr);
                refb[y, x] = new Gray(Cos(2 * PI * medianFreq * sigmaMaskX[y, x]));
                return true;
            });
            CvInvoke.Exp(refa, refa);

            double[,] refFilter = new double[filterSize, filterSize];
            MatTool<double>.Forward(ref refFilter, (y, x, val) => {
                refFilter[y, x] = refa[y, x].Intensity * refb[y, x].Intensity;
                return true;
            });

            var gaborFilter = new List<double[,]>();

            // rotate the filter
            for (int deg = 0; deg <= 180; deg += ANGLE_INC) {
                gaborFilter.Add(AffineRotation<double>.KeepSizeCreate(refFilter, -(deg + 90) / 180 * PI));
            }

            // Convert orientation matrix values from radians
            // to an index value that corresponds to round(degrees/angleInc)
            int maxOrientIndex = (int) Round(180f / ANGLE_INC);
            int[,] orientIndex = new int[orient.GetLength(0), orient.GetLength(1)];
            MatTool<double>.Forward(ref orient, (y, x, val) => {
                orientIndex[y, x] = (int) Round((val / PI * 180) / ANGLE_INC);
                return true;
            });

            // 
            MatTool<int>.Forward(ref orientIndex, (y, x, val) => {
                if (val <= 0)
                    orientIndex[y, x] = val + maxOrientIndex;
                if (val > maxOrientIndex)
                    orientIndex[y, x] = val - maxOrientIndex;
                return true;
            });

            //
            Iterator2D.Forward(blockSize, blockSize, h - blockSize, w - blockSize, (y, x) => {
                if (freq[y/imgBlockSize, x/imgBlockSize] <= 0)
                    return false;
                int angleInd = orientIndex[y / imgBlockSize, x / imgBlockSize] - 1;
                //
                for (int r = 0; r < filterSize; r++) {
                    for (int c = 0; c < filterSize; c++) {
                        int localY = y - blockSize + r;
                        int localX = x - blockSize + c;
                        res[r, c] += norm[localY, localX].Intensity * gaborFilter[angleInd][r, c];
                    }
                }
                return true;
            });

            // binary effect
            MatTool<double>.Forward(ref res, (y, x, val) => {
                if (val <= 0)
                    res[y, x] = 0;
                else
                    res[y, x] = 255;
                return true;
            });

            return res;
        }

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

            // get the median frequency
            freqList.Sort();
            return freqList.ToList()[freqList.Count >> 1];
        }
    }
}
