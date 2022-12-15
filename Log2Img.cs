using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Tool;
using FingerprintRecognition.MatrixConverter;
using System.Text;

namespace FingerprintRecognition {

    public class Log2Img {

        static readonly string LOC = "D:\\r\\siglaz\\FingerprintRecognition\\log\\";

        static public void Exc() {
            StreamReader ifs = new(new FileStream(LOC + "py-orient.log", FileMode.Open));
            string[] data = ifs.ReadToEnd().Split(' ');
            int cr = 0;

            int h = 480/16, w = 320/16;
            Image<Gray, double> img = new(w, h);
            Iterator2D.Forward(h, w, (y, x) => {
                double v = Convert.ToDouble(data[cr++]);
                img[y, x] = new Gray(v);
                return true;
            });

            CvInvoke.Imwrite(LOC + "py-orient.png", ToImage.FromDoubleImage(img));
        }
    }
}
