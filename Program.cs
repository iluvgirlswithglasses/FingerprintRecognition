
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Comparator;
using FingerprintRecognition.DataStructure;

/** @ program parameters */
const int BLOCK_SIZE = 16;
// the radius of the area that contains useful information
// measured relative to mask's width
const double USEFUL_RADIUS = 0.9;

/** @ temporary constants */
const string IN  = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";
const string OUT = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";

/** @ get files */
for (int i = 0; i < 10; i++) {
    Console.WriteLine("Processing file indexed " + i.ToString());
    FImage target = new(new Image<Gray, byte>(IN + string.Format("{0}.jpg", i)), BLOCK_SIZE, USEFUL_RADIUS);
    foreach (var j in target.SingularMgr.SingularLst)
        Console.WriteLine(j);
    // target.DisplaySingularity(i.ToString());
    // CvInvoke.Imwrite(string.Format(OUT + "mask-{0}.png", i), ToImage.FromBinaryArray(target.SegmentMask));
    Console.WriteLine();
}

