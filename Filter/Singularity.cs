
using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Filter {

    public class Singularity {
        readonly Bgr[] COLORS = {
            new Bgr(0, 0, 255),     // loop
            new Bgr(0, 128, 255),   // delta
            new Bgr(255, 128, 255), // whorl
        };

        public Image<Bgr, byte> Create(bool[,] ske, double[,] orient, int w, bool[,] msk) {
            var res = new Image<Bgr, byte>(ske.GetLength(1), ske.GetLength(0));
            Iterator2D.Forward(3, 3, orient.GetLength(0) - 2, orient.GetLength(1) - 2, (y, x) => {
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
                    int typ = PoinCare(orient, y, x);
                    if (typ != -1)
                        Iterator2D.Forward(y * w, x * w, y * w + w, x * w + w, (i, j) => {
                            res[i, j] = COLORS[typ];
                            return true;
                        });
                }
                return true;
            });
            return res;
        }

        public int PoinCare(double[,] orient, int y, int x) {
            // COLORS[i]: the color of code `i`
            return -1;
        }
    }
}
