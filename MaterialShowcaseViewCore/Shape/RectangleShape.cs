using Android.Graphics;

namespace MaterialShowcaseViewCore.Shape {
    public class RectangleShape:IShape {
        private readonly bool _fullWidth;

        

        private Rect _rect;

        public RectangleShape(int width, int height) {
            Width = width;
            Height = height;
            Init();
        }

        public RectangleShape(Rect bounds): this(bounds, false) {
            
        }

        public RectangleShape(Rect bounds, bool fullWidth) {
            _fullWidth = fullWidth;
            Height = bounds.Height();
            Width = fullWidth ? int.MaxValue : bounds.Width();
            Init();
        }

        public bool IsAdjustToTarget { get; set; } = true;

        private void Init() {
            _rect = new Rect(-Width / 2, -Height / 2, Width / 2, Height / 2);
        }

        public void Draw(Canvas canvas, Paint paint, int x, int y, int padding) {
            if (!_rect.IsEmpty) {
                canvas.DrawRect(
                    _rect.Left + x - padding,
                    _rect.Top + y - padding,
                    _rect.Right + x + padding,
                    _rect.Bottom + y + padding,
                    paint
                );
            }
        }

        public void UpdateTarget(Target.Target target) {
            if (!IsAdjustToTarget) return;
            var bounds = target.Bounds;
            Height = bounds.Height();
            Width = _fullWidth ? int.MaxValue : bounds.Width();
            Init();
        }

        public int Width { get;private set; }

        public int Height { get;private set; }
}
}