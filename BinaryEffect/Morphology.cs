
namespace FingerprintRecognition.BinaryEffect {

    internal class Morphology {

        /** @ shortcuts */
        static public void Open(ref bool[,] src) {
            src = MonoErosion(ref src);
            src = MonoDilation(ref src);
        }

        static public void Close(ref bool[,] src) {
            src = MonoDilation(ref src);
            src = MonoErosion(ref src);
        }

        /** @ features */
        static public bool[,] MonoDilation(ref bool[,] src) {
            int h = src.GetLength(0), w = src.GetLength(1);
            bool[,] res = new bool[h, w];

            for (int y = 1; y < h - 1; y++)
                for (int x = 1; x < w - 1; x++)
                    if (src[y, x])
                        MonoPlace(ref res, y, x, true);
            return res;
        }

        static public bool[,] MonoErosion(ref bool[,] src) {
            int h = src.GetLength(0), w = src.GetLength(1);
            bool[,]? res = src.Clone() as bool[,];

            if (res == null)
                return new bool[h, w];

            for (int y = 1; y < h - 1; y++)
                for (int x = 1; x < w - 1; x++)
                    if (!src[y, x])
                        MonoPlace(ref res, y, x, false);
            return res;
        }

        /** @ tools */
        static private void MonoPlace(ref bool[,] mat, int y, int x, bool v) {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    mat[y + i, x + j] = v;
        }
    }
}
