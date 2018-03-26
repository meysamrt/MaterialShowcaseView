using Android.Graphics;
using Math = System.Math;

namespace MaterialShowcaseViewCore.Shape {
    public class CircleShape:IShape {

        public CircleShape() {
        }

        public CircleShape(int radius) {
            Radius = radius;
        }

        public CircleShape(Rect bounds): this(GetPreferredRadius(bounds)) {
            
        }

        public CircleShape(Target.Target target): this(target.Bounds) {
        }

        public bool IsAdjustToTarget {  get; set; } = true;
       
        public int Radius { get; set; } = 200;
       
        public void Draw(Canvas canvas, Paint paint, int x, int y, int padding) {
            if (Radius > 0) {
                canvas.DrawCircle(x, y, Radius + padding, paint);
            }
        }

        
        public void UpdateTarget(Target.Target target) {
            if (IsAdjustToTarget)
                Radius = GetPreferredRadius(target.Bounds);
        }

        
        public int Width=> Radius * 2;
        

        
        public int Height=> Radius * 2;
        

        public static int GetPreferredRadius(Rect bounds) => Math.Max(bounds.Width(), bounds.Height()) / 2;
    }
}