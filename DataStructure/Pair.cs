
namespace FingerprintRecognition.DataStructure {
    public class Pair<A, B> where A: new() where B : new() {
        public A St;
        public B Nd;

        public Pair(A a, B b) {
            St = a;
            Nd = b;
        }

        public Pair() {
            St = new A();
            Nd = new B();
        }

        public override string ToString() {
            return String.Format("({0}, {1})", St, Nd);
        }
    }
}
