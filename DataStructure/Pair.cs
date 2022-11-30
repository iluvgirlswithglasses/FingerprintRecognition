
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
            return string.Format("({0}, {1})", St, Nd);
        }

        public override bool Equals(object? obj) {
            if (obj == null || St == null || Nd == null || !this.GetType().Equals(obj.GetType()))
                return false;
            Pair<A, B> x = (Pair<A, B>) obj;
            return St.Equals(x.St) && Nd.Equals(x.Nd);
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }
}
