using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FingerprintRecognition.Comparator;
using FingerprintRecognition.Filter;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.MathMatrix;

namespace FingerprintRecognition {

    // to use this,
    // erase everything in FImage init method

    public class BenchMark {

        /** @ program parameters */
        const int BLOCK_SIZE = 16;
        const double USEFUL_RADIUS = 1.0;
        const double ACCEPTED_MISMATCH = 0.095;
        const string IN = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";

        FImage[] Imgs;
        int Cnt;
        Stopwatch Counter;

        public BenchMark(int cnt) {
            Cnt = cnt;
            Imgs = new FImage[Cnt];
            for (int i = 0; i < Cnt; i++)
                Imgs[i] = new(
                    new Image<Gray, byte>(IN + i.ToString() + ".bmp"), BLOCK_SIZE, USEFUL_RADIUS
                );
        }

        private void Each(Func<FImage, bool> f) {
            for (int i = 0; i < Cnt; i++)
                f(Imgs[i]);
        }

        private void PrintElapse(string m) {
            TimeSpan t = Counter.Elapsed;
            Console.WriteLine(string.Format(
                m + ": {0}:{1}.{2}", t.Minutes, t.Seconds, t.Milliseconds
            ));
        }

        public void Run() {
            Console.WriteLine("Running Benchmark");
            Counter = new();

            Counter.Start();
            Each((i) => {
                i.Norm = Normalization.Normalize(i.Src, 100, 100);
                return true;
            });
            Counter.Stop();
            PrintElapse("First Normalization");

            Counter.Restart();
            Each((i) => {
                i.SegmentMask = Segmentation.CreateMask(i.Norm, BLOCK_SIZE);
                return true;
            });
            Counter.Stop();
            PrintElapse("First Segmentation");

            Counter.Restart();
            Each((i) => {
                Pair<int, int> maskMargin = SegmentationLyr2.GetMargin(i.Src, i.Norm, BLOCK_SIZE >> 1);
                Segmentation.CropMask(i.SegmentMask, maskMargin.St, maskMargin.Nd);
                // SegmentImg = Segmentation.ApplyMask(Norm, SegmentMask);
                return true;
            });
            PrintElapse("Second Segmentation");

            Counter.Restart();
            Each((i) => {
                i.Norm = Normalization.AllignAvg(i.Norm);
                i.ColorCenter = Segmentation.GetColorCenter(i.SegmentMask, i.Norm);
                i.OrientImg = OrientMat.Create(i.Norm, BLOCK_SIZE);
                return true;
            });
            PrintElapse("Orient Image");

            Counter.Restart();
            Each((i) => {
                int[,] singular = Singularity.Create(i.Norm.Height, i.Norm.Width, i.OrientImg, BLOCK_SIZE, i.SegmentMask);
                SingularityCleaner.ExcludeMixedPart(singular);
                SingularityCleaner.ExcludeSmallPart(singular, Convert.ToInt32(1.25 * i.ColorCenter.St), BLOCK_SIZE);
                i.SingularMgr = new(singular);
                return true;
            });
            PrintElapse("Singularities");

            Counter.Restart();
            Each((i) => {
                i.Norm = Normalization.ExcludeBackground(i.Norm, i.SegmentMask);
                i.FrequencyImg = RidgeFrequencyMat.Create(i.Norm, i.SegmentMask, i.OrientImg, BLOCK_SIZE, 5);
                return true;
            });
            PrintElapse("Frequency");

            Counter.Restart();
            Each((i) => {
                i.GaborImg = Gabor.Create(i.Norm, i.OrientImg, i.FrequencyImg, i.SegmentMask, BLOCK_SIZE);
                return true;
            });
            PrintElapse("Gabor filter");

            Counter.Restart();
            Each((i) => {
                ZhangSuen.ZhangSuenThinning(i.GaborImg);
                i.Skeleton = Binary.Create(i.GaborImg, 100);
                int margin = (int)Math.Round(3.0 / Gabor.GetMedianFreq(i.FrequencyImg) * 0.65);
                i.SingularMgr.ExtractKeysFromSkeleton(i.Skeleton, i.SegmentMask, margin);
                return true;
            });
            PrintElapse("Skeletonization");

            Counter.Restart();
            for (int i = 0; i < Cnt; i++) {
                for (int j = i+1; j < Cnt; j++) {
                    BruteComparator cmp = new(Imgs[i], Imgs[j], 8);
                }
            }
            PrintElapse(String.Format("Compared {0} pairs of images", (Cnt * (Cnt - 1)) >> 1));
        }
    }
}
