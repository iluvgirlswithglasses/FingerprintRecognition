using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Comparator;
using FingerprintRecognition.DataStructure;
using FingerprintRecognition.Filter;
using FingerprintRecognition.MatrixConverter;
using static System.Math;

/** @ program parameters */
const int BLOCK_SIZE = 16;
// the radius of the area that contains useful information
// measured relative to mask's width
const double USEFUL_RADIUS = 1.0;
const double ACCEPTED_MISMATCH = 0.095;

/** @ temporary constants */
const string IN  = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";
const string OUT = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";
const int START = 0, END = 3;

void RangeCompare() {
    /** @ get files */
    FImage[] imgs = new FImage[END];
    for (int i = START; i < END; i++) {
        Console.WriteLine("Processing file indexed " + i.ToString());
        imgs[i] = new(
            new Image<Gray, byte>(IN + string.Format("{0}.bmp", i)), BLOCK_SIZE, USEFUL_RADIUS
        );
        imgs[i].DisplaySingularity(i.ToString());
        Console.WriteLine();
    }

    /** @ compare files */
    AllImagesComparators cmp = new(imgs, START, END);
    // cmp.Compare(true, 0.35, 0.25, 0.25, 0.25);
    cmp.Compare(4, ACCEPTED_MISMATCH);
    cmp.MakeGroup();
    cmp.PrintGroups();
}

void TargetCompare() {
    int cnt = 2;
    string[] files = new string[]{"100.bmp", "101.bmp"};
    FImage[] imgs = new FImage[cnt];
    for (int i = 0; i < cnt; i++) {
        Console.WriteLine("Processing file " + files[i]);
        imgs[i] = new(
            new Image<Gray, byte>(IN + files[i]), BLOCK_SIZE, USEFUL_RADIUS
        );
        imgs[i].DisplaySingularity(files[i]);
        Console.WriteLine();
    }
    for (int i = 0; i < cnt; i++) {
        for (int j = i + 1; j < cnt; j++) {
            BruteComparator cmp = new(imgs[i], imgs[j], 4);
            Console.WriteLine(String.Format(
                "Comparing {0} and {1}:\nCA = {2}, CB = {3};\nRidge MMScore = {4}; Singu MMScore = {5};\nScore = {6} [{7}]\n", 
                i, j,
                cmp.CenterA, cmp.CenterB,
                cmp.RidgeMismatchScore, cmp.SingularityMismatchScore,
                cmp.CalcScore(), cmp.CalcScore() <= ACCEPTED_MISMATCH
            ));
        }
    }
}

/** @ main */
TargetCompare();
