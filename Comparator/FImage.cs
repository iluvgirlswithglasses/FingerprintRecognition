using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MathMatrix;
using FingerprintRecognition.MatrixConverter;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Comparator {

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
        int UsefulRadius;
        SingularityManager SingularMgr;

        public FImage(Image<Gray, byte> img, int bs, int usefulRad) {
            BlockSize = bs;
            UsefulRadius = usefulRad;
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
            Console.WriteLine("Trimming Mask");
            Segmentation.BFSTrim(SegmentMask, 5);
            // SegmentImg = Segmentation.ApplyMask(Norm, SegmentMask);

            // seperates the ridges
            Console.WriteLine("Second Normalizaion");
            Norm = Normalization.AllignAvg(Norm);

            // get ridges orient
            Console.WriteLine("Orientation");
            OrientImg = OrientMat.Create(Norm, BlockSize);
            Norm = Normalization.ExcludeBackground(Norm, SegmentMask);

            // get key points
            Console.WriteLine("Extracting Singularity");
            int[,] singular = Singularity.Create(Norm.Height, Norm.Width, OrientImg, BlockSize, SegmentMask);
            SingularMgr = new(singular, SegmentMask, UsefulRadius);

            // get frequency
            Console.WriteLine("Frequency");
            FrequencyImg = RidgeFrequencyMat.Create(Norm, SegmentMask, OrientImg, BlockSize, 5);

            // gabor filter, then skeletonization
            Console.WriteLine("Gabor filter");
            Skeleton = Binary.Create(Gabor.Create(Norm, OrientImg, FrequencyImg, SegmentMask, BlockSize), 100);
            Console.WriteLine("Skeletonization");
            new Skeletonization(Skeleton).Apply();

            // now that all the singularities are extracted
            // i'll keep the long ridges only
            Console.WriteLine("Removing Skeleton's noises");
            Skeletonization.RemoveShortRidges(Skeleton, 20);
            SingularMgr.ExtractKeysFromSkeleton(Skeleton);
            CvInvoke.Imwrite(DEBUG + "skeleton.png", ToImage.FromBinaryArray(Skeleton));
        }

        public void DisplaySingularity() {
            Image<Bgr, byte> res = new(Norm.Size);

            Iterator2D.Forward(1, 1, res.Height - 1, res.Width - 1, (y, x) => {
                int adj = 0;
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        if (SingularMgr.Mat[y + i, x + j] != -1) adj++;
                if (SingularMgr.Mat[y, x] != -1 && adj <= 8) {
                    res[y, x] = Singularity.COLORS[SingularMgr.Mat[y, x]];
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
