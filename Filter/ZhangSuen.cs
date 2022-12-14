using System;
using System.Threading.Tasks;

namespace FingerprintRecognition.Filter
{
    /// <summary>
    /// Implements skeletonization by Zhang-Suen Thinning algorithm
    /// </summary>
    static class ZhangSuen
    {
        public static int MaxIterations = 7;

        /// <summary>
        /// Skeletonization by Zhang-Suen Thinning algorithm
        /// </summary>
        /// <param name="data">Pixels of image: byte[width, height, 1], where black is 0, white is 255</param>
        public static void ZhangSuenThinning(byte[,] src)
        {
            var tmp = (byte[,])src.Clone();
            int cnt = 0;
            int iterations = MaxIterations;
            do  // the missing iteration
            {
                cnt = Step(1, tmp, src);
                tmp = (byte[,])src.Clone();
                cnt += Step(2, tmp, src);
                tmp = (byte[,])src.Clone();
                iterations--;
            }
            while (cnt > 0 && iterations > 0);
        }

        static int Step(int stepNo, byte[,] tmp, byte[,] src)
        {
            int cnt = 0;
            var width = tmp.GetLength(1);
            var height = tmp.GetLength(0);

            Parallel.For((int)1, width - 1, x =>
            {
                for (int y = 1; y < height - 1; y++)
                if (src[y, x] > 0 && SuenThinningAlg(x, y, tmp, stepNo == 2))
                {
                    cnt++;
                    src[y, x] = 0;
                }
            });
            return cnt;
        }

        static bool SuenThinningAlg(int x, int y, byte[,] src, bool even)
        {
            var p2 = src[y - 1, x];
            var p3 = src[y - 1, x + 1];
            var p4 = src[y, x + 1];
            var p5 = src[y + 1, x + 1];
            var p6 = src[y + 1, x];
            var p7 = src[y + 1, x - 1];
            var p8 = src[y, x - 1];
            var p9 = src[y - 1, x - 1];

            //calc NumberOfNonZeroNeighbors
            int bp1 = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;

            if (bp1 >= 2 * 255 && bp1 <= 6 * 255) //2nd condition
            {
                //clac NumberOfZeroToOneTransitionFromP9
                int bp2 = (~p2 & p3) + (~p3 & p4) + (~p4 & p5) + (~p5 & p6) + (~p6 & p7) + (~p7 & p8) + (~p8 & p9) + (~p9 & p2);

                if (bp2 == 255)
                {
                    if (even)
                        return (p2 & p4 & p8) == 0 && (p2 & p6 & p8) == 0;
                    else
                        return (p2 & p4 & p6) == 0 && (p4 & p6 & p8) == 0;
                }
            }
            return false;
        }
    }
}