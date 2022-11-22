using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Filter {

    internal class Binary {

        static public bool[,] Create(double[,] src, double threshold) {
            bool[,] res = new bool[src.GetLength(0), src.GetLength(1)];
            MatTool<double>.Forward(ref src, (y, x, v) => {
                res[y, x] = v >= threshold;
                return true;
            });
            return res;
        }
    }
}
