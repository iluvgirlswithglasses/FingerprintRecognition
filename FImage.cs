using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition
{
    internal class FImage
    {
        public Image<Gray, byte> Src;

        public FImage(Image<Gray, byte> img)
        {
            Src = img;
        }
    }
}
