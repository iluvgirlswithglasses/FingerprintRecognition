using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.MathMatrix
{
    internal class RidgeFrequencyMat
    {
        static public double[,] Create(ref Image<Gray, double> norm, ref bool[,] msk, ref double[,] orient, int blockSize, int kernelSize, int minWaveLength, int maxWavelength)
        {
            double[,] res = new double[msk.GetLength(0), msk.GetLength(1)];

            return res;
        }
    }
}
