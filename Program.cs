
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MatConverter;

/** temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\";
const string SOURCE = CWD + "sample-images\\0.jpg";
const string TARGET = CWD + "sample-images\\2.jpg";

const string O1PATH = CWD + "sample-images-o\\blured-normalized.jpg";
const string O2PATH = CWD + "sample-images-o\\blured-normalized--binary.jpg";

/** envokes */
FImage source = new FImage(new Image<Gray, byte>(SOURCE));
FImage target = new FImage(new Image<Gray, byte>(TARGET));

/** debug */
target.Src = Smooth.LibBlur(ref target.Src);
Image<Gray, double> normDouble = Normalization.Normalize(ref target.Src, 100.0, 100.0);

CvInvoke.Imwrite(O1PATH, ToImage.FromDoubleMatrix(ref normDouble));
