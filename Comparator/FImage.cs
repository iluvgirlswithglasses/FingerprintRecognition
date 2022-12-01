using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MathMatrix;
using FingerprintRecognition.MatrixConverter;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Comparator {

    public class FImage {

        const string DEBUG = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";

        /** @ img matrix */
        int BlockSize;
        public Image<Gray, byte> Src;

        /** @ pre-processing */
        public Image<Gray, double> Norm;
        // Image<Gray, double> SegmentImg;
        public bool[,] SegmentMask;
        public double[,] OrientImg;
        public double[,] FrequencyImg;
        public bool[,] Skeleton;

        /** @ singularity matrices */
        Pair<int, int> Center;
        int UsefulRadius;
        public SingularityManager SingularMgr;

        public FImage(Image<Gray, byte> img, int bs, double usefulRad) {
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
            // Console.WriteLine("Trimming Mask");
            // Segmentation.BFSTrim(SegmentMask, 5);
            // SegmentImg = Segmentation.ApplyMask(Norm, SegmentMask);

            // crop the masks
            Console.WriteLine("Cropping the mask");
            Norm = Normalization.AllignAvg(Norm);
            Center = Segmentation.GetCenter(SegmentMask, Norm);
            UsefulRadius = Convert.ToInt32(usefulRad * (Segmentation.GetMaskWidth(SegmentMask) >> 1));
            Segmentation.CropMask(SegmentMask, Center, UsefulRadius);

            // get ridges orient
            Console.WriteLine("Orientation");
            OrientImg = OrientMat.Create(Norm, BlockSize);
            Norm = Normalization.ExcludeBackground(Norm, SegmentMask);

            // get key points
            Console.WriteLine("Extracting Singularity");
            int[,] singular = Singularity.Create(Norm.Height, Norm.Width, OrientImg, BlockSize, SegmentMask);
            SingularMgr = new(singular);

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
        }

        public void DisplaySingularity(string fname) {
            Image<Bgr, byte> res = new(Norm.Size);

            Iterator2D.Forward(1, 1, res.Height - 1, res.Width - 1, (y, x) => {
                int typ = SingularMgr.Mat[y, x];
                if (typ != -1) {
                    int adj = 0;
                    for (int i = -1; i <= 1; i++)
                        for (int j = -1; j <= 1; j++)
                            if (SingularMgr.Mat[y + i, x + j] == typ) adj++;

                    if (adj <= 6) {
                        res[y, x] = Singularity.COLORS[SingularMgr.Mat[y, x]];
                        return true;
                    }
                }

                int c = 255 * Convert.ToInt32(Skeleton[y, x]);
                res[y, x] = new Bgr(c, c, c);
                return true;
            });
            CvInvoke.Imwrite(DEBUG + string.Format("singular-{0}.png", fname), res);
        }
    }
}
