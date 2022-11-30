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
        msk:    the segmentation mask, size [h, w]
        orient: the orient matrix, size [h/bs, w/bs]
        bs:     block size
        ks:     kernel size
        */
        static public double[,] Create(Image<Gray, double> norm, bool[,] msk, double[,] orient, int bs, int ks, int minWaveLength=5, int maxWavelength=15) {
            double[,] freq = new double[norm.Height, norm.Width];

            Iterator2D.Forward(orient.GetLength(0), orient.GetLength(1), (y, x) => {
                double angle = orient[y, x];
                if (angle != 0) {
                    double val = Query(
                        norm, y * bs, x * bs, Min(y * bs + bs, norm.Height), Min(x * bs + bs, norm.Width), angle, ks, minWaveLength, maxWavelength
                    );
                    if (val != 0.0) {
                        MatTool<double>.Forward(ref freq, y * bs, x * bs, y * bs + bs, x * bs + bs, (r, c, _v) => {
                            freq[r, c] = val;
                            return true;
                        });
                    }
                }
                return true;
            });

            MatTool<double>.Forward(ref freq, (y, x, v) => {
                freq[y, x] = v * Convert.ToInt32(msk[y, x]);
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

            int width = rotated.GetLength(1);

            // sum down the projection
            double[] ridgeSum = new double[width];
            for (int x = 0; x < rotated.GetLength(1); x++)
                for (int y = 0; y < rotated.GetLength(0); y++)
                    ridgeSum[x] += rotated[y, x];

            // some statistics
            double avg = 0.0;
            for (int x = 0; x < width; x++)
                avg += ridgeSum[x];
            avg /= ridgeSum.Length;

            // get dilation and filter out noise
            double[] dilation = Morphology.SimpleGrayDilation(ridgeSum, ks, 1);
            double[] ridgeNoise = new double[dilation.Length];
            for (int x = 0; x < width; x++)
                ridgeNoise[x] = Abs(dilation[x] - ridgeSum[x]);

            // get peaks
            List<int> peaks = new List<int>();
            for (int x = 0; x < width; x++)
                if (ridgeNoise[x] < 2 && ridgeSum[x] > avg)
                    peaks.Add(x);

            // Determine the spatial frequency of the ridges by dividing the
            // distance between the 1st and last peaks by the (No of peaks-1). If no
            // peaks are detected, or the wavelength is outside the allowed bounds, the frequency image is set to 0
            if (peaks.Count < 2)
                return 0.0;
            double waveLength = (double)(peaks.Last() - peaks.First()) / (peaks.Count - 1);
            if (minWaveLength <= waveLength && waveLength <= maxWaveLength)
                return 1.0 / waveLength;
            return 0.0;
        }
    }
}
