using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.Filter
{
    internal class Binary
    {
        static public Image<Gray, byte> Create(ref Image<Gray, byte> src, double threshold)
        {
            var res = new Image<Gray, byte>(src.Size);
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    res[y, x] = new Gray(255.0 * Convert.ToInt32(src[y, x].Intensity > threshold));
            return res;
        }
    }
}
