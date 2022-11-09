using Emgu.CV;
using Emgu.CV.Structure;

namespace FingerprintRecognition.MatConverter
{
    internal class ToImage
    {
        static public Image<Gray, byte> FromBinaryArray(ref bool[,] bin)
        {
            var res = new Image<Gray, byte>(bin.GetLength(0), bin.Length);
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray(255.0 * Convert.ToInt32(bin[y, x]));
            return res;
        }

        static public Image<Gray, byte> FromDoubleMatrix(ref Image<Gray, double> src)
        {
            var res = new Image<Gray, byte>(src.Size);
            double mx = Tool.MatTool<double>.Max(ref src);

            if (mx == 0.0)
                return res;

            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[y, x] = new Gray(src[y, x].Intensity / mx * 255.0);
            return res;
        }
    }
}
