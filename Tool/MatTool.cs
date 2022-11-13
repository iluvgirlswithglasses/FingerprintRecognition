
namespace FingerprintRecognition.Tool
{
    internal class MatTool<T> where T : new()
    {
        /** @ the core */
        static public void Forward(ref T[,] src, Func<int, int, T, bool> f)
        {
            Forward(ref src, 0, 0, src.GetLength(0), src.GetLength(1), (y, x, v) => { return true; }, f);
        }

        static public void Forward(ref T[,] src, int t, int l, int d, int r, Func<int, int, T, bool> f)
        {
            Forward(ref src, t, l, d, r, (y, x, v) => { return true; }, f);
        }

        static public void Forward(ref T[,] src, Func<int, int, T, bool> g, Func<int, int, T, bool> f)
        {
            Forward(ref src, 0, 0, src.GetLength(0), src.GetLength(1), g, f);
        }

        static public void Forward(
            ref T[,] src,
            int t, int l, int d, int r,
            Func<int, int, T, bool> g,
            Func<int, int, T, bool> f
        )
        {
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    // if g() then execute f()
                    if (g(y, x, src[y, x]))
                        f(y, x, src[y, x]);
        }
    }
}
