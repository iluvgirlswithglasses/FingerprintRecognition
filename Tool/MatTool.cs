﻿using Emgu.CV;
using Emgu.CV.Structure;
using static System.Math;

namespace FingerprintRecognition.Tool
{
    internal class MatTool<TDepth> where TDepth : new()
    {
        /** @ tools for byte images */
        static public double Std(ref Image<Gray, TDepth> src)
        {
            /*
            The standard deviation is the square root of the average of the 
            squared deviations from the mean, i.e., std = sqrt(mean(x)), 
            where x = abs(a - a.mean())**2.
            */
            double avg = src.GetAverage().Intensity;
            double res = 0;
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    res += Sqr(src[y, x].Intensity - avg);

            return Sqrt(res / (src.Height * src.Width));
        }

        static public double Max(ref Image<Gray, TDepth> src)
        {
            double mx = -1e18;
            for (int y = 0; y < src.Height; y++)
                for (int x = 0; x < src.Width; x++)
                    mx = Math.Max(mx, src[y, x].Intensity);
            return mx;
        }

        /** @ calculators */
        static private double Sqr(double x)
        {
            return x * x;
        }
    }
}
