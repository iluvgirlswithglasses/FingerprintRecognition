using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MathMatrix;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition {

    internal class FImage {

        const string DEBUG = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";

        /** @ img matrix */
        int BlockSize;
        public Image<Gray, byte> Src;

        /** @ pre-processing */
        Image<Gray, double> Norm;
        // Image<Gray, double> SegmentImg;
        bool[,] SegmentMask;
        double[,] OrientImg;
        double[,] FrequencyImg;
        bool[,] Skeleton;

        /** @ singularity matrices */
        int[,] Singular;

        public FImage(Image<Gray, byte> img, int bs) {
            BlockSize = bs;
            Src = new(img.Size);
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    Src[y, x] = new Gray(255 - img[y, x].Intensity);

            // make the image smooth, both shape-wise and color-wise
            // target.Src = Smooth.LibBlur(ref target.Src);
            Norm = Normalization.Normalize(Src, 100.0, 100.0);

            // focus on the fingerprint
            SegmentMask = Segmentation.CreateMask(Norm, BlockSize);
            // SegmentImg = Segmentation.ApplyMask(Norm, SegmentMask);
            // seperates the ridges
            Norm = Normalization.AllignAvg(Norm);
            // get gradient image
            OrientImg = OrientMat.Create(Norm, BlockSize);
            Norm = Normalization.ExcludeBackground(Norm, SegmentMask);
            // get frequency
            FrequencyImg = RidgeFrequencyMat.Create(Norm, SegmentMask, OrientImg, BlockSize, 5);
            // gabor filter, then skeletonization
            Skeleton = Binary.Create(Gabor.Create(Norm, OrientImg, FrequencyImg, SegmentMask, BlockSize), 100);
            new Skeletonization(Skeleton).Apply();
            // get key points
            Singular = Singularity.Create(Skeleton, OrientImg, BlockSize, SegmentMask);
        }

        public void DisplaySingularity() {
            Image<Bgr, byte> res = new(Norm.Size);
            Iterator2D.Forward(res.Height, res.Width, (y, x) => {
                if (Singular[y, x] != -1)
                    res[y, x] = Singularity.COLORS[Singular[y, x]];
                else {
                    int c = 255 * Convert.ToInt32(Skeleton[y, x]);
                    res[y, x] = new Bgr(c, c, c);
                }
                return true;
            });
            CvInvoke.Imwrite(DEBUG + "singularity.png", res);
        }
    }
}
