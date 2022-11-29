
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
// measured in pixel
const int USEFUL_RADIUS = 145;

/** @ temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\";
const string SOURCE = CWD + "sample-images\\2.jpg";
const string TARGET = CWD + "sample-images\\2.jpg";

/** @ get files */
// FImage source = new(new Image<Gray, byte>(SOURCE), BLOCK_SIZE);
FImage target = new(new Image<Gray, byte>(TARGET), BLOCK_SIZE, USEFUL_RADIUS);

target.DisplaySingularity();
