﻿using FingerprintRecognition.Algorithm;
using FingerprintRecognition.Tool;

namespace FingerprintRecognition.Filter {

    public class Skeletonization {
        bool[,] Img;
        int height, width;

        /*
        adj cells look like this:

        0 1 2
        7   3
        6 5 4
        */
        readonly int[] Y = { -1, -1, -1, 0, 1, 1, 1, 0 };
        readonly int[] X = { -1, 0, 1, 1, 1, 0, -1, -1 };

        public Skeletonization(bool[,] src) {
            Img = src;
            height = Img.GetLength(0);
            width = Img.GetLength(1);
        }

        public void Apply() {
            Deque<KeyValuePair<int, int>> queue = new();
            // get the outline first
            Iterator2D.Forward(2, 2, height - 2, width - 2, (y, x) => {
                if (!Img[y, x]) Bleed(ref queue, y, x);
                return true;
            });
            // bfs
            while (queue.Count > 0) {
                var p = queue.First();
                queue.RemoveFromFront();
                // test
                if (CanRemove(p.Key, p.Value)) {
                    Img[p.Key, p.Value] = false;
                    Bleed(ref queue, p.Key, p.Value);
                }
            }
        }

        private void Bleed(ref Deque<KeyValuePair<int, int>> queue, int y, int x) {
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    if (Img[y + i, x + j]) queue.AddToBack(new(y + i, x + j));
        }

        private bool CanRemove(int y, int x) {
            int adj = GetAdj(y, x), shf = GetShift(y, x);

            return 
                (2 <= adj && adj <= 6) &&
                (shf == 1) &&
                (!GetRelativeCell(y, x, 0) || !GetRelativeCell(y, x, 2) || !GetRelativeCell(y, x, 6) || GetShift(y + Y[0], x + X[0]) != 1) &&
                (!GetRelativeCell(y, x, 0) || !GetRelativeCell(y, x, 2) || !GetRelativeCell(y, x, 4) || GetShift(y + Y[2], x + X[2]) != 1);
        }

        private bool GetRelativeCell(int y, int x, int i) {
            return Img[y + Y[i], x + X[i]];
        }

        private int GetAdj(int y, int x) {
            int adj = 0;
            for (int i = 0; i < 8; i++)
                if (Img[y + Y[i], x + X[i]])
                    adj++;
            // the ridge of a fingerprint only split from 1 to 2 path 
            return adj;
        }

        private int GetShift(int y, int x) {
            int shf = 0;
            for (int i = 0; i < 8; i++) {
                if (!Img[y + Y[i], x + X[i]] && Img[y + Y[AdjMod(i + 1)], x + X[AdjMod(i + 1)]])
                    shf++;
            }
            return shf;
        }

        static private int AdjMod(int i) {
            return i >= 8 ? i - 8 : i;
        }
    }
}