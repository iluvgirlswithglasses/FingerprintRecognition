
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Comparator;

/** @ program parameters */
const int BLOCK_SIZE = 16;
// the radius of the area that contains useful information
// measured relative to mask's width
const double USEFUL_RADIUS = 0.9;

/** @ temporary constants */
const string IN  = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";
const string OUT = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";
const int IM_COUNT = 11;

/** @ get files */
FImage[] imgs = new FImage[IM_COUNT];
for (int i = 0; i < IM_COUNT; i++) {
    Console.WriteLine("Processing file indexed " + i.ToString());
    imgs[i] = new(new Image<Gray, byte>(IN + string.Format("{0}.jpg", i)), BLOCK_SIZE, USEFUL_RADIUS);
    imgs[i].DisplaySingularity(i.ToString());
    Console.WriteLine();
}

/** @ compare files */
for (int i = 0; i < IM_COUNT; i++) {
    for (int j = i + 1; j < IM_COUNT; j++) {
        SingularityComparator cmp = new SingularityComparator(
            imgs[i].SingularMgr, imgs[j].SingularMgr, imgs[i].Skeleton, imgs[j].Skeleton
        );
        Console.WriteLine(
            "Img {0} & {1}:\nSinguls match = {2}, angle diff = {3}, dist diff = {4}, ridges mismatch score = {5}\n", 
            i, j, cmp.SMatches, cmp.SAngleMismatchScore, cmp.SLenMismatchScore, cmp.SingulRidgesMismatchScore
        );
    }
}
