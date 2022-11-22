
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition;

/** @ program parameters */
const int BLOCK_SIZE = 16;

/** @ temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\";
const string SOURCE = CWD + "sample-images\\2.jpg";
const string TARGET = CWD + "sample-images\\2.jpg";

/** @ get files */
FImage source = new(new Image<Gray, byte>(SOURCE));
FImage target = new(new Image<Gray, byte>(TARGET));

target.PreprocessProcedure(BLOCK_SIZE);
