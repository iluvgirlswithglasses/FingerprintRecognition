using System;
using System.Threading.Tasks;

namespace SmartVector.Model
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
        public static void ZhangSuenThinning(byte[,,] data)
        {
            var temp = (byte[,,])data.Clone();
            int count = 0;
            int iterations = MaxIterations;
            do  // the missing iteration
            {
                count = Step(1, temp, data);
                temp = (byte[,,])data.Clone();
                count += Step(2, temp, data);
                temp = (byte[,,])data.Clone();
                iterations--;
            }
            while (count > 0 && iterations > 0);
        }

        static int Step(int stepNo, byte[,,] temp, byte[,,] data)
        {
            int count = 0;
            var width = temp.GetLength(1);
            var height = temp.GetLength(0);

            Parallel.For((int)1, width - 1, x =>
            {
                for (int y = 1; y < height - 1; y++)
                if (SuenThinningAlg(x, y, temp, stepNo == 2))
                {
                    // still changes happening?
                    if (data[y, x, 0] > 0) count++;
                    data[y, x, 0] = 0;
                }
            });
            return count;
        }

        static bool SuenThinningAlg(int x, int y, byte[,,] data, bool even)
        {
            var p2 = data[y - 1, x, 0];
            var p3 = data[y - 1, x + 1, 0];
            var p4 = data[y, x + 1, 0];
            var p5 = data[y + 1, x + 1, 0];
            var p6 = data[y + 1, x, 0];
            var p7 = data[y + 1, x - 1, 0];
            var p8 = data[y, x - 1, 0];
            var p9 = data[y - 1, x - 1, 0];

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