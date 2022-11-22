
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

        public Image<Bgr, byte> Create(bool[,] ske, double[,] orient, int blocksize, bool[,] msk) {
            var res = new Image<Bgr, byte>(ske.GetLength(1), ske.GetLength(0));
            Iterator2D.Forward(3, 3, orient.GetLength(0) - 2, orient.GetLength(1) - 2, (y, x) => {
                int t = (y - 2) * blocksize, 
                    l = (x - 2) * blocksize, 
                    d = (y + 3) * blocksize, 
                    r = (x + 3) * blocksize;
                int sum = 0;
                MatTool<bool>.Forward(ref msk, (_y, _x, v) => {
                    if (v) sum++;
                    return true;
                });
                // if all of this region is inside the mask
                if (sum == (5*blocksize) * (5*blocksize)) {
                    
                }
                return true;
            });
            return res;
        }
    }
}
