using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.DataStructure;
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
            Console.WriteLine("First Normalization");
            Norm = Normalization.Normalize(Src, 100.0, 100.0);

            // focus on the fingerprint
            Console.WriteLine("Creating Mask");
            SegmentMask = Segmentation.CreateMask(Norm, BlockSize);
            // SegmentImg = Segmentation.ApplyMask(Norm, SegmentMask);

            // seperates the ridges
            Console.WriteLine("Second Normalizaion");
            Norm = Normalization.AllignAvg(Norm);

            // get gradient image
            Console.WriteLine("Orientation");
            OrientImg = OrientMat.Create(Norm, BlockSize);
            Norm = Normalization.ExcludeBackground(Norm, SegmentMask);

            // get frequency
            Console.WriteLine("Frequency");
            FrequencyImg = RidgeFrequencyMat.Create(Norm, SegmentMask, OrientImg, BlockSize, 5);

            // gabor filter, then skeletonization
            Console.WriteLine("Gabor filter");
            Skeleton = Binary.Create(Gabor.Create(Norm, OrientImg, FrequencyImg, SegmentMask, BlockSize), 100);
            Console.WriteLine("Skeletonization");
            new Skeletonization(Skeleton).Apply();

            // get key points
            Console.WriteLine("Singularity");
            Singular = Singularity.Create(Skeleton, OrientImg, BlockSize, SegmentMask);
            for (int i = 0; i < Singularity.COLORS.Length; i++) {
                Pair<int, int> tmp = Singularity.KeepCenterMost(Singular, i);
                Console.WriteLine(tmp);
            }
        }

        public void DisplaySingularity() {
            Image<Bgr, byte> res = new(Norm.Size);
            
            Iterator2D.Forward(1, 1, res.Height - 1, res.Width - 1, (y, x) => {
                int adj = 0;
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        if (Singular[y + i, x + j] != -1) adj++;
                if (4 <= adj && adj < 9) {
                    res[y, x] = Singularity.COLORS[Singular[y, x]];
                } else {
                    int c = 255 * Convert.ToInt32(Skeleton[y, x]);
                    res[y, x] = new Bgr(c, c, c);
                }
                return true;
            });
            CvInvoke.Imwrite(DEBUG + "singularity.png", res);
        }
    }
}
