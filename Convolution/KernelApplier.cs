using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.Convolution
{
    internal class KernelApplier
    {
        public static Image<Gray, double> Apply(ref Image<Gray, double> src, ref int[,] kernel)
        {
            var res = new Image<Gray, double>(src.Size);
            for (int y = 1; y < res.Height - 1; y++)
                for (int x = 1; x < res.Width - 1; x++)
                    res[y, x] = new Gray();
            return res;
        }
    }
}
