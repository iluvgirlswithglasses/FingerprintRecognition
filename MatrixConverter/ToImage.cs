using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.MatrixConverter {

    internal class ToImage {

        static public Image<Gray, byte> FromBinaryArray(bool[,] bin) {
            var res = new Image<Gray, byte>(bin.GetLength(1), bin.GetLength(0));
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray(255.0 * Convert.ToInt32(bin[y, x]));
            return res;
        }

        static public Image<Gray, byte> FromDoubleMatrix(double[,] src) {
            var res = new Image<Gray, byte>(src.GetLength(1), src.GetLength(0));

            double mn = 0.0, mx = 0.0, span;
            MatTool<double>.Forward(ref src, (y, x, val) => {
                mx = Math.Max(mx, val);
                mn = Math.Min(mn, val);
                return true;
            });

            span = mx - mn;
            if (span == 0)
                return res;

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray((src[y, x] - mn) / span * 255.0);
            return res;
        }

        static public Image<Gray, byte> FromDoubleImage(Image<Gray, double> src) {
            var res = new Image<Gray, byte>(src.Size);

            double mn = 0.0, mx = 0.0, span;
            ImgTool<double>.Forward(ref src, (y, x, val) => {
                mx = Math.Max(mx, val);
                mn = Math.Min(mn, val);
                return true;
            });

            span = mx - mn;
            if (span == 0)
                return res;

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray((src[y, x].Intensity - mn) / span * 255.0);
            return res;
        }
    }
}
