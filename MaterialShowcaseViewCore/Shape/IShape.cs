using Android.Graphics;

namespace MaterialShowcaseViewCore.Shape {
    public interface  IShape {
        void Draw(Canvas canvas, Paint paint, int x, int y, int padding);

        /**
         * Get width of the shape.
         */
        int Width { get; }

        /**
         * Get height of the shape.
         */
        int Height { get; }

        /**
         * Update shape bounds if necessary
         */
        void UpdateTarget(Target.Target target);
    }
}