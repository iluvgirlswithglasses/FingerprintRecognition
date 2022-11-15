using static System.Math;

namespace FingerprintRecognition.Transform {
    /*
    references:
        https://en.wikipedia.org/wiki/Affine_transformation
        https://en.wikipedia.org/wiki/Transformation_matrix#Rotation
        https://www.algorithm-archive.org/contents/affine_transformations/affine_transformations.html

    the idea of the matrix given in wiki is that:
        given a point P, 
        after multiplying it with the matrix M, 
        we got P' which is the point after rotation

    so with every point P(y, x), after transformation it becomes P'(y', x') where:
        y' = x * sin(a) + y * cos(a)
        x' = x * cos(a) - y * sin(a)

    reversed approach:
        y = - x * sin(a) + y * cos(a)
        x = + x * cos(a) + y * sin(a)
    */

    internal class AffineRotation<TDepth> where TDepth : new() {

        // keep size
        static public TDepth[,] Create(TDepth[,] src, double rad) {
            rad = -rad;

            int h = src.GetLength(0), w = src.GetLength(1);
            int cy = h >> 1, cx = w >> 1;

            TDepth[,] res = new TDepth[h, w];
            
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    // given P', calculate P
                    int py = y - cy, px = x - cx;
                    int dy = cy + (int)Round(- px * Sin(rad) + py * Cos(rad)),
                        dx = cx + (int)Round(+ px * Cos(rad) + py * Sin(rad));

                    if (0 <= dy && dy < h && 0 <= dx && dx < w)
                        res[y, x] = src[dy, dx];
                }
            }
            //
            return res;
        }
    }
}
