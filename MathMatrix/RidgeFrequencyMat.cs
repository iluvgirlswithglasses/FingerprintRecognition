using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.MatrixConverter;
using FingerprintRecognition.Tool;
using FingerprintRecognition.Transform;
using static System.Math;

namespace FingerprintRecognition.MathMatrix {

    internal class RidgeFrequencyMat {

        /*
        norm:   normalized image after excluding background, size [h, w]
        msk:    the segmentation mask, size [h/bs, w/bs]
        orient: the orient matrix, size [h/bs, w/bs]
        bs:     block size
        ks:     kernel size
        */
        static public double[,] Create(Image<Gray, double> norm, bool[,] msk, double[,] orient, int bs, int ks, int minWaveLength=5, int maxWavelength=15) {
            int h = msk.GetLength(0), w = msk.GetLength(1);
            double[,] freq = new double[h, w];

            Iterator2D.Forward(h, w, (y, x) => {
                double angle = orient[y, x];
                if (angle != 0) {
                    freq[y, x] = Query(
                        norm, y * bs, x * bs, Min(y * bs + bs, norm.Height), Min(x * bs + bs, norm.Width), angle, ks, minWaveLength, maxWavelength
                    );
                }
                return true;
            });

            return freq;
        }

        // references: https://pdfs.semanticscholar.org/ca0d/a7c552877e30e1c5d87dfcfb8b5972b0acd9.pdf pg.14
        static private double Query(Image<Gray, double> norm, int t, int l, int d, int r, double orient, int ks, int minWaveLength, int maxWaveLength) {
            int h = d - t, w = r - l;

            // Find mean orientation within the block
            double cosOrient = Cos(2.0 * orient);
            double sinOrient = Sin(2.0 * orient);
            double blockOrient = Atan2(sinOrient, cosOrient) / 2.0;

            // Rotate the image block so that the ridges are vertical
            double[,] cropped = ImgTool<double>.Crop(ref norm, t, l, d, r);
            double[,] rotated = AffineRotation<double>.KeepSizeCreate(cropped, blockOrient + Acos(0));

            // Crop the image so that the rotated image does not contain any invalid regions.
            int cropSize = (int)Floor(Convert.ToDouble(h) / Sqrt(2));
            int cropOffset = (h - cropSize) >> 1;
            rotated = MatTool<double>.Crop(
                ref rotated, cropOffset, cropOffset, cropOffset + cropSize, Min(cropOffset + cropSize, w)
            );

            // NOTE: Very unsafe
            double avg = 0.0;
            MatTool<double>.Forward(ref rotated, (y, x, val) => {
                avg += val;
                return true;
            });
            avg /= rotated.GetLength(0) * rotated.GetLength(1);
            MatTool<double>.Forward(ref rotated, (y, x, val) => {
                if (val <= avg)
                    rotated[y, x] = 0;
                else
                    rotated[y, x] = 255;
                return true;
            });

            int cnt = RidgeCount(rotated);
            if (cnt < 2)
                return 0.0;
            return (double) cnt / rotated.GetLength(1);

            // debug
            // const string SAVE_DIR = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\blocks\\";
            // CvInvoke.Imwrite(SAVE_DIR + String.Format("{0}-{1}-cropped.png", t, l), ToImage.FromDoubleMatrix(cropped));
            // CvInvoke.Imwrite(SAVE_DIR + String.Format("{0}-{1}-rotated.png", t, l), ToImage.FromDoubleMatrix(rotated));
        }

        static private int RidgeCount(double[,] block) {
            int mx = 0;
            for (int y = 0; y < block.GetLength(0); y++) {
                int cr = 0;
                if (block[y, 0] == 255)
                    cr = 1;
                for (int x = 1; x < block.GetLength(1); x++)
                    if (block[y, x - 1] == 0 && block[y, x] == 255)
                        cr++;
                mx = Max(mx, cr);
            }
            return mx;
        }
    }
}
