using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition {
    internal class FImage {
        public Image<Gray, byte> Src;

        public FImage(Image<Gray, byte> img) {
            Src = new(img.Size);
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    Src[y, x] = new Gray(255 - img[y, x].Intensity);
        }
    }
}
