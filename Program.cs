using Emgu.CV;
using Emgu.CV.Structure;
using FingerprintRecognition.Comparator;

/** @ program parameters */
const int BLOCK_SIZE = 16;
// the radius of the area that contains useful information
// measured relative to mask's width
const double USEFUL_RADIUS = 1.0;

/** @ temporary constants */
const string IN  = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images\\";
const string OUT = "D:\\r\\siglaz\\FingerprintRecognition\\sample-images-o\\";
const int IM_COUNT = 66;
const int START = 0, END = 11;

/** @ get files */
FImage[] imgs = new FImage[IM_COUNT];
for (int i = START; i < END; i++) {
    Console.WriteLine("Processing file indexed " + i.ToString());
    imgs[i] = new(
        new Image<Gray, byte>(IN + string.Format("{0}.jpg", i)), BLOCK_SIZE, USEFUL_RADIUS
    );
    imgs[i].DisplaySingularity(i.ToString());
    Console.WriteLine();
}

/** @ compare files */
AllImagesComparators cmp = new(imgs, START, END);
cmp.Compare(true, 0.35, 0.25, 0.25, 0.25, 0.25);
cmp.PrintGroups();

