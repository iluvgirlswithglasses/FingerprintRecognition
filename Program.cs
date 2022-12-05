using System.Reflection;
using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Comparator;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MatrixConverter;
using static System.Net.Mime.MediaTypeNames;

/** @ program parameters */
const int BLOCK_SIZE = 16;
// the radius of the area that contains useful information
// measured relative to mask's width
const double USEFUL_RADIUS = 1.0;

/** @ temporary constants */
const string IN  = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";
const string OUT = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";
const int IM_COUNT = 66;
const int START = 0, END = 1;

/** @ get files */
FImage[] imgs = new FImage[IM_COUNT];
for (int i = START; i < END; i++) {
    Console.WriteLine("Processing file indexed " + i.ToString());
    imgs[i] = new(
        new Image<Gray, byte>(IN + string.Format("{0}.jpg", i)), BLOCK_SIZE, USEFUL_RADIUS
    );
    imgs[i].DisplaySingularity(i.ToString());
    Console.WriteLine();
}

/** @ compare files */
double inc = 10 * Math.PI / 180;
for (double ang = 0; ang < Math.PI * 2; ang += inc) {
    Pair<int, int> c = new(240, 160);
    double fa = Math.Cos(ang), fb = -Math.Sin(ang);
    int dist = BruteComparator.GetDist(imgs[0].SegmentMask, c, fa, fb);
    BruteComparator.CountRidges(c, fa, fb, dist, imgs[0].Skeleton);
}
for (int i = -2; i <= 2; i++) {
    for (int j = -2; j <= 2; j++) {
        imgs[0].Skeleton[240 + i, 160 + j] = true;
    }
}
CvInvoke.Imwrite(OUT + "deb.png", ToImage.FromBinaryArray(imgs[0].Skeleton));
// AllImagesComparators cmp = new(imgs, START, END);
// cmp.Compare(true, 0.35, 0.25, 0.25, 0.25);
// cmp.BruteCompare(50, 4, 10, 0.25);
// cmp.PrintGroups();

