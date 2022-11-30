
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using FingerprintRecognition.Comparator;

/** @ program parameters */
const int BLOCK_SIZE = 16;
// the radius of the area that contains useful information
// measured relative to mask's width
const double USEFUL_RADIUS = 0.95;

/** @ temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";

/** @ get files */
for (int i = 0; i < 10; i++) {
    Console.WriteLine("\nProcessing file indexed " + i.ToString());
    FImage target = new(new Image<Gray, byte>(CWD + string.Format("{0}.jpg", i)), BLOCK_SIZE, USEFUL_RADIUS);
    target.DisplaySingularity(i.ToString());
}

