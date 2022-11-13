using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.MathMatrix
{
    internal class RidgeFrequencyMat
    {
        static public double[,] Create(Image<Gray, double> norm, bool[,] msk, double[,] orient, int blockSize, int kernelSize, int minWaveLength, int maxWavelength)
        {
            double[,] res = new double[msk.GetLength(0), msk.GetLength(1)];

            return res;
        }
    }
}
