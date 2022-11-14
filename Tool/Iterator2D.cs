
namespace FingerprintRecognition.Tool
{
    internal class Iterator2D
    {
        /** @ the core */
        static public void Forward(int h, int w, Func<int, int, bool> f)
        {
            Forward(0, 0, h, w, f);
        }

        static public void Forward(int t, int l, int d, int r, Func<int, int, bool> f)
        {
            for (int y = t; y < d; y++)
                for (int x = l; x < r; x++)
                    f(y, x);
        }
    }
}
