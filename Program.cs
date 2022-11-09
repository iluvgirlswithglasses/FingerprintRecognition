
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition;
using FingerprintRecognition.Filter;

/** temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\";
const string SOURCE = CWD + "sample-images\\0.jpg";
const string TARGET = CWD + "sample-images\\2.jpg";
const string O_PATH = CWD + "sample-images-o\\blured-normalized.jpg";

/** envokes */
FImage source = new FImage(new Image<Gray, byte>(SOURCE));
FImage target = new FImage(new Image<Gray, byte>(TARGET));

/** debug */
target.Src = Smooth.LibBlur(ref target.Src);
Image<Gray, double> normDouble = Normalization.Normalize(ref target.Src, 100.0, 100.0);
Image<Gray, byte> normByte = FingerprintRecognition.MatConverter.ToImage.FromDoubleMatrix(ref normDouble);

CvInvoke.Imwrite(O_PATH, normByte);
