using Android.App;
using Android.Graphics;
using Android.Views;

namespace MaterialShowcaseViewCore.Target {
    public class ViewTarget:Target {
        private readonly View _view;

        public ViewTarget(View view) {
            _view = view;
        }

        public ViewTarget(int viewId, Activity activity) {
            _view = activity.FindViewById(viewId);
        }

        public override Point Point {
            get {
                var location = new int[2];
                _view.GetLocationInWindow(location);
                var x = location[0] + _view.Width / 2;
                var y = location[1] + _view.Height / 2;
                return new Point(x, y);
            }
        }

        public override Rect Bounds {
            get {
                var location = new int[2];
                _view.GetLocationInWindow(location);
                return new Rect(
                    location[0],
                    location[1],
                    location[0] + _view.MeasuredWidth,
                    location[1] + _view.MeasuredHeight
                );
            }
        }
    }
}