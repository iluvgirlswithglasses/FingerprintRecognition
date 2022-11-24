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

        /** @ */
        int BLOCK_SIZE;
        Image<Gray, double> norm;
        bool[,] segmentMask;
        Image<Gray, double> segmented;
        double[,] orient;
        double[,] freq;
        bool[,] gabor;

        public FImage(Image<Gray, byte> img, int bs) {
            BLOCK_SIZE = bs;
            Src = new(img.Size);
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    Src[y, x] = new Gray(255 - img[y, x].Intensity);
        }

        public void PreprocessProcedure() {
            // make the image smooth, both shape-wise and color-wise
            // target.Src = Smooth.LibBlur(ref target.Src);
            norm = Normalization.Normalize(Src, 100.0, 100.0);

            // focus on the fingerprint
            segmentMask = Segmentation.CreateMask(norm, BLOCK_SIZE);
            segmented = Segmentation.ApplyMask(norm, segmentMask);
            // seperates the ridges
            norm = Normalization.AllignAvg(norm);
            // get gradient image
            orient = OrientMat.Create(norm, BLOCK_SIZE);
            norm = Normalization.ExcludeBackground(norm, segmentMask);
            // get frequency
            freq = RidgeFrequencyMat.Create(norm, segmentMask, orient, BLOCK_SIZE, 5);
            // gabor filter
            gabor = Binary.Create(Gabor.Create(norm, orient, freq, segmentMask, BLOCK_SIZE), 100);
            // skeletonization
            new Skeletonization(gabor).Apply();
        }

        public Image<Bgr, byte> DetectSingularity() {
            Image<Bgr, byte> res = Singularity.Create(gabor, orient, BLOCK_SIZE, segmentMask);
            CvInvoke.Imwrite(DEBUG + "pts.png", res);
            return res;
        }
    }
}
