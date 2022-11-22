using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MathMatrix;
using FingerprintRecognition.MatrixConverter;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition {

    internal class FImage {

        const string DEBUG = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";

        /** @ img matrix */
        public Image<Gray, byte> Src;

        public FImage(Image<Gray, byte> img) {
            Src = new(img.Size);
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    Src[y, x] = new Gray(255 - img[y, x].Intensity);
        }

        public void PreprocessProcedure(int BLOCK_SIZE) {
            // make the image smooth, both shape-wise and color-wise
            // target.Src = Smooth.LibBlur(ref target.Src);
            Image<Gray, double> norm = Normalization.Normalize(Src, 100.0, 100.0);

            // focus on the fingerprint
            bool[,] segmentMask = Segmentation.CreateMask(norm, BLOCK_SIZE);
            Image<Gray, double> segmented = Segmentation.ApplyMask(norm, segmentMask);
            // seperates the ridges
            norm = Normalization.AllignAvg(norm);
            // get gradient image
            double[,] orient = OrientMat.Create(norm, BLOCK_SIZE);
            norm = Normalization.ExcludeBackground(norm, segmentMask);

            // get frequency
            double[,] freq = RidgeFrequencyMat.Create(norm, segmentMask, orient, BLOCK_SIZE, 5);

            // gabor filter
            bool[,] gabor = Binary.Create(
                Gabor.Create(norm, orient, freq, segmentMask, BLOCK_SIZE), 100
            );
            CvInvoke.Imwrite(DEBUG + "beforeBFS.png", ToImage.FromBinaryArray(gabor));

            // skeletonization
            new Skeletonization(gabor).Apply();
            CvInvoke.Imwrite(DEBUG + "afterBFS.png", ToImage.FromBinaryArray(gabor));
        }
    }
}
