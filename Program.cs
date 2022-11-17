
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition;
using FingerprintRecognition.Convolution;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MathMatrix;
using FingerprintRecognition.MatrixConverter;
using FingerprintRecognition.Tool;
using FingerprintRecognition.Transform;

/** @ program parameters */
const int BLOCK_SIZE = 16;

/** @ temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\";
const string SOURCE = CWD + "sample-images\\2.jpg";
const string TARGET = CWD + "sample-images\\2.jpg";

/** @ get files */
FImage source = new(new Image<Gray, byte>(SOURCE));
FImage target = new(new Image<Gray, byte>(TARGET));

/** @ procedure */
// make the image smooth, both shape-wise and color-wise
// target.Src = Smooth.LibBlur(ref target.Src);
Image<Gray, double> norm = Normalization.Normalize(target.Src, 100.0, 100.0);

// focus on the fingerprint
bool[,] segmentMask = Segmentation.CreateMask(norm, BLOCK_SIZE);
Image<Gray, double> segmented = Segmentation.ApplyMask(norm, segmentMask);
// seperates the ridges
norm = Normalization.AllignAvg(norm);
// get gradient image
double[,] orient = OrientMat.Create(norm, BLOCK_SIZE);
norm = Normalization.ExcludeBackground(norm, segmentMask);

// get frequency
double[,] freq = RidgeFrequencyMat.Create(norm, segmentMask, orient, BLOCK_SIZE, 5);

// gabor filter
double[,] gabor = Gabor.Create(norm, orient, freq, segmentMask, BLOCK_SIZE);

/** @ debug */
CvInvoke.Imwrite(CWD + "sample-images-o\\normalized.jpg", norm);
CvInvoke.Imwrite(CWD + "sample-images-o\\mask.jpg", ToImage.FromBinaryArray(segmentMask));
CvInvoke.Imwrite(CWD + "sample-images-o\\segmented.jpg", ToImage.FromDoubleImage(segmented));
CvInvoke.Imwrite(CWD + "sample-images-o\\gabor-filter.jpg", ToImage.FromDoubleMatrix(gabor));
