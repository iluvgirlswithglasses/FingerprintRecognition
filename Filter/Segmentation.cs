using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.Filter
{
    internal class Segmentation
    {
        static public Image<Gray, double> Create(ref Image<Gray, double> src)
        {
            var res = new Image<Gray, double>(src.Size);
            var msk = res.Copy();

            return res;
        }
    }
}
