using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using static System.Math;

namespace FingerprintRecognition.Filter {

    internal class Normalization {

        static public Image<Gray, double> Normalize(Image<Gray, byte> src, double m0, double v0) {
            var res = new Image<Gray, double>(src.Size);
            double m = src.GetAverage().Intensity, 
                   v = ImgTool<byte>.Std(ref src);

            ImgTool<byte>.Forward(ref src, (y, x, val) => {
                res[y, x] = new Gray(Floor(NormalizePixel(m0, v0, val, m, v)));
                return true;
            });

            return res;
        }

        static private double NormalizePixel(double m0, double v0, double px, double m, double v) {
            double coeff = Sqrt( v0 * ((px - m) * (px - m))) / v;
            if (px > m)
                return m0 + coeff;
            return m0 - coeff;
        }

        static public Image<Gray, double> AllignAvg(Image<Gray, double> norm) {
            var res = new Image<Gray, double>(norm.Size);

            double avg = norm.GetAverage().Intensity;
            double std = ImgTool<double>.Std(ref norm);

            ImgTool<double>.Forward(ref norm, (y, x, v) => {
                res[y, x] = new Gray((v - avg) / std);
                return true;
            });

            return res;
        }

        static public Image<Gray, double> ExcludeBackground(Image<Gray, double> norm, bool[,] msk, int w) {
            var res = new Image<Gray, double>(norm.Size);

            int n = 0;
            double sum = 0.0, avg = 0.0, std = 0.0;

            MatTool<bool>.Forward(ref msk, (y, x, v) => {
                if (!v) {
                    ImgTool<double>.Forward(ref norm, y*w, x*w, y*w + w, x*w + w, (_y, _x, val) => {
                        sum += val;
                        n++;
                        return true;
                    });
                }
                return true;
            });

            avg = sum / n;

            MatTool<bool>.Forward(ref msk, (y, x, v) => {
                if (!v)
                    std += ImgTool<double>.Sum(
                        ref norm, y*w, x*w, y*w + w, x*w + w, (x) => { return (x - avg) * (x - avg); }
                    );
                return true;
            });

            std = Sqrt(std / n);

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray((norm[y, x].Intensity - avg) / std);
            
            return res;
        }

        /** @ useless prototype methods */
        static public Image<Gray, double> BinaryNormalize(Image<Gray, byte> src, double threshold) {
            var res = new Image<Gray, double>(src.Size);
            double m = src.GetAverage().Intensity * threshold;

            ImgTool<byte>.Forward(ref src, (y, x, val) => {
                res[y, x] = new Gray(val > m ? 255 : 0);
                return true;
            });

            return res;
        }
    }
}
