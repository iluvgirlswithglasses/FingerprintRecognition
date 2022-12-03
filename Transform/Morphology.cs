
namespace FingerprintRecognition.Transform {

    internal class Morphology<T> where T: new() {

        /** @ features */
        static public T[,] MonoDilation(T[,] src, T target, Func<T, T, bool> cmp) {
            int h = src.GetLength(0), w = src.GetLength(1);

            T[,] res = src.Clone() as T[,];

            for (int y = 1; y < h - 1; y++)
                for (int x = 1; x < w - 1; x++)
                    if (cmp(src[y, x], target))
                        MonoPlace(res, y, x, target);
            return res;
        }

        /** @ grayscale */
        static public double[] SimpleGrayDilation(double[] src, int kernelSize, int v) {
            int r = (kernelSize - 1) >> 1, l = src.Length;
            double[] res = new double[l];
            
            for (int i = 0; i < l; i++) {
                for (int j = Math.Max(0, i-r); j <= Math.Min(i+r, l-1); j++) {
                    res[j] = Math.Max(res[j], src[i] + v);
                }
            }
            return res;
        }

        /** @ tools */
        static private void MonoPlace(T[,] mat, int y, int x, T v) {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    mat[y + i, x + j] = v;
        }
    }
}
