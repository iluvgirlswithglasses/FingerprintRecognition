
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

/** get files */
FImage source = new(new Image<Gray, byte>(SOURCE));
FImage target = new(new Image<Gray, byte>(TARGET));

/** procedure */
target.Src = Smooth.LibBlur(ref target.Src);
Image<Gray, double> norm = Normalization.Normalize(ref target.Src, 100.0, 100.0);
Image<Gray, double> segmented = Segmentation.Create(ref norm, 16);

/** debug */
norm = Normalization.AllignAvg(ref norm);
CvInvoke.Imwrite(CWD + "sample-images-o\\norm.jpg", ToImage.FromDoubleMatrix(ref norm));
CvInvoke.Imwrite(CWD + "sample-images-o\\segmented.jpg", ToImage.FromDoubleMatrix(ref segmented));
