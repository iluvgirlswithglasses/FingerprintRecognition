
/** 
 * this program checks whether the TARGET fingerprint
 * matches the SOURCE fingerprint in the database
 */

using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MathMatrix;
using FingerprintRecognition.MatrixConverter;

/** @ program parameters */
const int BLOCK_SIZE = 8;

/** @ temporary constants */
const string CWD    = "D:\\r\\siglaz\\FingerprintRecognition\\";
const string SOURCE = CWD + "sample-images\\0.jpg";
const string TARGET = CWD + "sample-images\\2.jpg";

/** @ get files */
FImage source = new(new Image<Gray, byte>(SOURCE));
FImage target = new(new Image<Gray, byte>(TARGET));

/** @ procedure */
// make the image smooth, both shape-wise and color-wise
// target.Src = Smooth.LibBlur(ref target.Src);
Image<Gray, double> norm = Normalization.Normalize(ref target.Src, 100.0, 100.0);
// focus on the fingerprint
bool[,] segmentMask = Segmentation.CreateMask(ref norm, BLOCK_SIZE);
Image<Gray, double> segmented = Segmentation.ApplyMask(ref norm, ref segmentMask, BLOCK_SIZE);
// seperates the ridges
norm = Normalization.AllignAvg(ref norm);
// get gradient image
double[,] orient = OrientMat.Create(ref norm, BLOCK_SIZE);

/** @ debug */
CvInvoke.Imwrite(CWD + "sample-images-o\\segmented.jpg", ToImage.FromDoubleMatrix(ref segmented));
Image<Gray, byte> orientImg = OrientMat.Visualize(ref segmented, ref segmentMask, ref orient, BLOCK_SIZE);
CvInvoke.Imwrite(CWD + "sample-images-o\\orient-img.jpg", orientImg);
