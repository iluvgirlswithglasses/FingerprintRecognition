using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.MathMatrix
{
    internal class RidgeFrequencyMat
    {
        /*
        norm:   normalized image after excluding background, size [h, w]
        msk:    the segmentation mask, size [h/bs, w/bs]
        orient: the orient matrix, size [h/bs, w/bs]
        bs:     block size
        ks:     kernel size
        */
        static public double[,] Create(Image<Gray, double> norm, bool[,] msk, double[,] orient, int bs, int ks, int minWaveLength=5, int maxWavelength=15)
        {
            int h = norm.Height, w = norm.Width;
            double[,] freq = new double[h, w];

            Iterator2D.Forward(orient.GetLength(0), orient.GetLength(1), (y, x) => {
                double angle = orient[y, x];
                if (angle != 0)
                {
                    Iterator2D.Forward(y*bs, x*bs, Min(y*bs + bs, h), Min(x*bs + bs, w), (y, x) => {

                        return true;
                    });
                }
                return true;
            });

            return freq;
        }
    }
}
