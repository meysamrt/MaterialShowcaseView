using Android.Graphics;

namespace MaterialShowcaseViewCore.Shape {
    public class NoShape:IShape {

        public void UpdateTarget(Target.Target target) {
            // do nothing
        }

        public void Draw(Canvas canvas, Paint paint, int x, int y, int padding) {
            // do nothing
        }

       

        public int Width => 0;

        public int Height => 0;
    }
}