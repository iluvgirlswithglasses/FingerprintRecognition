
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
const string SOURCE = CWD + "sample-images\\0.jpg";
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
Image<Gray, double> segmented = Segmentation.ApplyMask(norm, segmentMask, BLOCK_SIZE);
// seperates the ridges
norm = Normalization.AllignAvg(norm);
// get gradient image
double[,] orient = OrientMat.Create(norm, BLOCK_SIZE);
norm = Normalization.ExcludeBackground(norm, segmentMask, BLOCK_SIZE);
// get frequency
double[,] freq = RidgeFrequencyMat.Create(norm, segmentMask, orient, BLOCK_SIZE, 5);

/** @ debug */
CvInvoke.Imwrite(CWD + "sample-images-o\\normalized.jpg", norm);
CvInvoke.Imwrite(CWD + "sample-images-o\\segmented.jpg", ToImage.FromDoubleImage(segmented));
Image<Gray, byte> orientImg = OrientMat.Visualize(segmented, segmentMask, orient, BLOCK_SIZE);
CvInvoke.Imwrite(CWD + "sample-images-o\\orient-img.jpg", orientImg);

MatTool<double>.Forward(ref freq, (y, x, val) => {
    if (val > 0)
        Console.WriteLine(String.Format("{0}-{1} f = {2}", y * BLOCK_SIZE, x * BLOCK_SIZE, val));
    return true;
});
