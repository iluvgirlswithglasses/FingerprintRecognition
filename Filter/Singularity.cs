using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter {

    public class Singularity {

        public static readonly Bgr[] COLORS = {
            new Bgr(0, 0, 255),     // 0: loop
            new Bgr(0, 128, 255),   // 1: delta
            new Bgr(255, 128, 255), // 2: whorl
        };

        static readonly KeyValuePair<int, int>[] RELATIVE = {
            // this goes round
            new(-1, -1), new(-1, +0), new(-1, +1), new(+0, +1), new(+1, +1), new(+1, +0), new(+1, -1), new(+0, -1), new(-1, -1)
        };

        static public int[,] Create(bool[,] ske, double[,] orient, int w, bool[,] msk) {
            int[,] res = new int[ske.GetLength(0), ske.GetLength(1)];
            MatTool<int>.Forward(ref res, (y, x, v) => {
                res[y, x] = -1;
                return true;
            });
            //
            Iterator2D.Forward(3, 3, orient.GetLength(0) - 3, orient.GetLength(1) - 3, (y, x) => {
                int t = (y - 2) * w, 
                    l = (x - 2) * w, 
                    d = (y + 3) * w, 
                    r = (x + 3) * w;
                int mskSum = 0;
                MatTool<bool>.Forward(ref msk, t, l, d, r, (_y, _x, v) => {
                    if (v) mskSum++;
                    return true;
                });
                // if all of this region is inside the mask
                if (mskSum == (5*w) * (5*w)) {
                    int typ = PoinCare(orient, y, x, 1);
                    Iterator2D.Forward(y * w, x * w, y * w + w, x * w + w, (i, j) => {
                        res[i, j] = typ;
                        return true;
                    });
                }
                return true;
            });
            return res;
        }

        static public int PoinCare(double[,] orient, int y, int x, int margin) {
            double[] surroundingAngle = new double[9];
            for (int i = 0; i < 9; i++)
                surroundingAngle[i] = orient[y + RELATIVE[i].Key, x + RELATIVE[i].Value] / PI * 180;
            double total = 0;
            for (int i = 0; i < 8; i++) {
                double diff = surroundingAngle[i] - surroundingAngle[i+1];
                if (diff > 90)
                    diff -= 180;
                else if (diff < -90)
                    diff += 180;
                total += diff;
            }

            // COLORS[i]: the color of code `i`
            if (180 - margin <= total && total <= 180 + margin)
                return 0;
            if (-180 - margin <= total && total <= -180 + margin)
                return 1;
            if (360 - margin <= total && total <= 360 + margin)
                return 2;
            return -1;
        }
    }
}
