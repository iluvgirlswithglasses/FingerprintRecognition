using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.Filter {

    internal class Smooth {

        // remove this later
        static public Image<Gray, byte> LibBlur(ref Image<Gray, byte> src) {
            return src.SmoothGaussian(3);
        }
    }
}
