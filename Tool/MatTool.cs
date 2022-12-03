
namespace FingerprintRecognition.Tool {

    internal class MatTool<T> where T : new() {

        /** @ the core */
        static public void Forward(ref T[,] src, Func<int, int, T, bool> f) {
            Forward(ref src, 0, 0, src.GetLength(0), src.GetLength(1), f);
        }

        static public void Forward(ref T[,] src, int t, int l, int d, int r, Func<int, int, T, bool> f) {
            t = Max(0, t);
            l = Max(0, l);
            d = Min(src.GetLength(0), d);
            r = Min(src.GetLength(1), r);
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    f(y, x, src[y, x]);
        }

        /** @ extensions */
        static public T[,] Crop(ref T[,] src, int t, int l, int d, int r) {
            T[,] res = new T[d - t, r - l];
            for (int i = 0; i < res.GetLength(0); i++)
                for (int j = 0; j < res.GetLength(1); j++)
                    res[i, j] = src[t + i, l + j];
            return res;
        }

        static public void FromOuterBFSSetup(ref T[,] src, Func<int, int, bool> f, int margin=0) {
            int h = src.GetLength(0), w = src.GetLength(1);
            for (int x = margin; x < w - margin; x++) {
                f(margin, x);
                f(h - 1 - margin, x);
            }
            for (int y = margin; y < h - margin; y++) {
                f(y, margin);
                f(y, w - 1 - margin);
            }
        }

        /** @ i don't want to include System.Math */
        static private int Min(int a, int b) {
            return a < b ? a : b;
        }

        static private int Max(int a, int b) {
            return a > b ? a : b;
        }
    }
}
