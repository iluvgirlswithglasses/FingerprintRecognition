
namespace FingerprintRecognition.Convolution {

    internal class SobelOperator {
        
        // [-1, 0, 1], [-2, 0, 2], [-1, 0, 1]
        static public readonly int[,] X_KERNEL = {
            {-1, +0, +1 },
            {-2, +0, +2 },
            {-1, +0, +1 },
        };

        static public readonly int[,] Y_KERNEL = {
            {-1, -2, -1 },
            {+0, +0, +0},
            {+1, +2, +1 }
        };
    }
}
