using System.Collections.Generic;
using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MaterialShowcaseViewCore.Shape;
using MaterialShowcaseViewCore.Target;
using Math = System.Math;

namespace MaterialShowcaseViewCore {
    public enum ShapeType {
        Circle,
        Rectangle,
        NoShape
    }
    public class MaterialShowcaseView : FrameLayout, ViewTreeObserver.IOnGlobalLayoutListener, View.IOnTouchListener, View.IOnClickListener {
        private int _oldHeight;
        private int _oldWidth;
        private Bitmap _bitmap;// = new WeakReference<>(null);
        private Canvas _canvas;
        private Paint _eraser;
        private Target.Target _target;
        private IShape _shape;
        private bool _wasDismissed;
        private int _shapePadding = ShowcaseConfig.DefaultShapePadding;

        private View _contentBox;
        private TextView _titleTextView;
        private TextView _contentTextView;
        private TextView _dismissButton;
        private GravityFlags _gravity;
        private int _contentBottomMargin;
        private int _contentTopMargin;
        private bool _dismissOnTouch;
        private bool _shouldRender; // flag to decide when we should actually render
        private bool _renderOverNav;
        private Color _maskColour;
        private IAnimationFactory _animationFactory;
        private bool _shouldAnimate = true;
        private bool _useFadeAnimation;
        private long _fadeDurationInMillis = ShowcaseConfig.DefaultFadeTime;
        private Handler _handler;
        private long _delayInMillis = ShowcaseConfig.DefaultDelay;
        private int _bottomMargin;
        private bool _singleUse; // should display only once
        private PrefsManager _prefsManager; // used to store state doe single use mode
        List<IShowcaseListener> _listeners; // external listeners who want to observe when we show and dismiss
        //private UpdateOnGlobalLayout mLayoutListener;
        private IDetachedListener _detachedListener;
        private bool _targetTouchable;
        private bool _dismissOnTargetTouch = true;

        public MaterialShowcaseView(Context context) : base(context) {
            Init();
        }

        public MaterialShowcaseView(Context context, IAttributeSet attrs) : base(context, attrs) {
            Init();
        }

        public MaterialShowcaseView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
            Init();
        }

        [TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
        public MaterialShowcaseView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
            Init();
        }


        private void Init() {
            SetWillNotDraw(false);

            _listeners = new List<IShowcaseListener>();

            // make sure we add a global layout listener so we can adapt to changes
            //mLayoutListener = new UpdateOnGlobalLayout(this,mTarget);
            ViewTreeObserver.AddOnGlobalLayoutListener(this);

            // consume touch events
            SetOnTouchListener(this);

            _maskColour = ShowcaseConfig.DefaultMaskColour;
            Visibility = ViewStates.Invisible;
            // setVisibility(INVISIBLE);


            var contentView = LayoutInflater.From(Context).Inflate(Resource.Layout.showcase_content, this, true);
            _contentBox = contentView.FindViewById(Resource.Id.content_box);
            _titleTextView = contentView.FindViewById<TextView>(Resource.Id.tv_title);
            _contentTextView = contentView.FindViewById<TextView>(Resource.Id.tv_content);
            _dismissButton = contentView.FindViewById<TextView>(Resource.Id.tv_dismiss);
            _dismissButton.SetOnClickListener(this);
        }


        /**
         * Interesting drawing stuff.
         * We draw a block of semi transparent colour to fill the whole screen then we draw of transparency
         * to create a circular "viewport" through to the underlying content
         *
         * @param canvas
         */
        protected override void OnDraw(Canvas canvas) {
            base.OnDraw(canvas);

            // don't bother drawing if we're not ready
            if (!_shouldRender) return;

            // get current dimensions
            var width = MeasuredWidth;
            var height = MeasuredHeight;

            // don't bother drawing if there is nothing to draw on
            if (width <= 0 || height <= 0) return;

            // build a new canvas if needed i.e first pass or new dimensions
            if (_bitmap == null || _canvas == null || _oldHeight != height || _oldWidth != width) {
                _bitmap?.Recycle();

                _bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

                _canvas = new Canvas(_bitmap);
            }

            // save our 'old' dimensions
            _oldWidth = width;
            _oldHeight = height;

            // clear canvas
            _canvas.DrawColor(Color.Transparent, PorterDuff.Mode.Clear);

            // draw solid background
            _canvas.DrawColor(_maskColour);

            // Prepare eraser Paint if needed
            if (_eraser == null) {
                _eraser = new Paint {Color = Color.ParseColor("#FFFFFFFF")};
                _eraser.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));
                _eraser.Flags = PaintFlags.AntiAlias;
            }

            // draw (erase) shape
            _shape.Draw(_canvas, _eraser, Position.X, Position.Y, _shapePadding);

            // Draw the bitmap on our views  canvas.
            canvas.DrawBitmap(_bitmap, 0, 0, null);
        }

        protected override void OnDetachedFromWindow() {
            base.OnDetachedFromWindow();

            /**
             * If we're being detached from the window without the mWasDismissed flag then we weren't purposefully dismissed
             * Probably due to an orientation change or user backed out of activity.
             * Ensure we reset the flag so the showcase display again.
             */
            if (!_wasDismissed && _singleUse)
                _prefsManager?.ResetShowcase();



            NotifyOnDismissed();

        }

        public bool OnTouch(View v, MotionEvent @event) {
            if (_dismissOnTouch) {
                Hide();
            }
            if (!_targetTouchable || !_target.Bounds.Contains((int) @event.GetX(), (int) @event.GetY())) return true;
            if (_dismissOnTargetTouch) {
                Hide();
            }
            return false;
        }


        private void NotifyOnDisplayed() {
            if (_listeners == null) return;
            foreach (var listener in _listeners) {
                listener.OnShowcaseDisplayed(this);
            }
        }

        private void NotifyOnDismissed() {
            if (_listeners != null) {
                foreach (var listener in _listeners) 
                    listener.OnShowcaseDismissed(this);

                _listeners.Clear();
                _listeners = null;
            }

            /**
             * internal listener used by sequence for storing progress within the sequence
             */
            _detachedListener?.OnShowcaseDetached(this, _wasDismissed);
        }

        /**
         * Dismiss button clicked
         *
         * @param v
         */
        public void OnClick(View v) {
            Hide();
        }

        /**
         * Tells us about the "Target" which is the view we want to anchor to.
         * We figure out where it is on screen and (optionally) how big it is.
         * We also figure out whether to place our content and dismiss button above or below it.
         *
         * @param target
         */
        public void SetTarget(Target.Target target) {
            _target = target;

            // update dismiss button state
            UpdateDismissButton();

            if (_target != null) {

                /**
                 * If we're on lollipop then make sure we don't draw over the nav bar
                 */
                if (!_renderOverNav && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) {
                    _bottomMargin = GetSoftButtonsBarSizePort((Activity)Context);
                    var contentLp = (LayoutParams)LayoutParameters;

                    if (contentLp != null && contentLp.BottomMargin != _bottomMargin)
                        contentLp.BottomMargin = _bottomMargin;
                }

                // apply the target position
                var targetPoint = _target.Point;
                var targetBounds = _target.Bounds;
                Position = targetPoint;

                // now figure out whether to put content above or below it
                var height = MeasuredHeight;
                var midPoint = height / 2;
                var yPos = targetPoint.Y;

                var radius = Math.Max(targetBounds.Height(), targetBounds.Width()) / 2;
                if (_shape != null) {
                    _shape.UpdateTarget(_target);
                    radius = _shape.Height / 2;
                }

                if (yPos > midPoint) {
                    // target is in lower half of screen, we'll sit above it
                    _contentTopMargin = 0;
                    _contentBottomMargin = (height - yPos) + radius + _shapePadding;
                    _gravity = GravityFlags.Bottom;
                } else {
                    // target is in upper half of screen, we'll sit below it
                    _contentTopMargin = yPos + radius + _shapePadding;
                    _contentBottomMargin = 0;
                    _gravity = GravityFlags.Top;
                }
            }

            ApplyLayoutParams();
        }

        private void ApplyLayoutParams() {
            if (_contentBox?.LayoutParameters == null) return;
            var contentLp = (LayoutParams)_contentBox.LayoutParameters;

            var layoutParamsChanged = false;

            if (contentLp.BottomMargin != _contentBottomMargin) {
                contentLp.BottomMargin = _contentBottomMargin;
                layoutParamsChanged = true;
            }

            if (contentLp.TopMargin != _contentTopMargin) {
                contentLp.TopMargin = _contentTopMargin;
                layoutParamsChanged = true;
            }

            if (contentLp.Gravity != _gravity) {
                contentLp.Gravity = _gravity;
                layoutParamsChanged = true;
            }

            /**
                 * Only apply the layout params if we've actually changed them, otherwise we'll get stuck in a layout loop
                 */
            if (layoutParamsChanged)
                _contentBox.LayoutParameters = contentLp;
        }

        /**
         * SETTERS
         */

        public Point Position { get; set; }


        private void SetTitleText(string contentText) {
            if (_titleTextView == null || contentText.Equals("")) return;
            _contentTextView.Alpha = 0.5F;
            _titleTextView.Text = contentText;
        }

        private void SetContentText(string contentText) {
            if (_contentTextView != null)
                _contentTextView.Text = contentText;

        }

        private void SetDismissText(string dismissText) {
            if (_dismissButton == null) return;
            _dismissButton.Text = dismissText;
            UpdateDismissButton();
        }

        private void SetDismissStyle(Typeface dismissStyle) {
            if (_dismissButton == null) return;
            _dismissButton.Typeface = dismissStyle;
            UpdateDismissButton();
        }

        private void SetTitleTextColor(Color textColour) => _titleTextView?.SetTextColor(textColour);

        private void SetContentTextColor(Color textColour) => _contentTextView?.SetTextColor(textColour);

        private void SetDismissTextColor(Color textColour) => _dismissButton?.SetTextColor(textColour);

        private void SetShapePadding(int padding) => _shapePadding = padding;

        private void SetDismissOnTouch(bool dismissOnTouch) => _dismissOnTouch = dismissOnTouch;

        private void SetShouldRender(bool shouldRender) => _shouldRender = shouldRender;

        private void SetMaskColour(Color maskColour) => _maskColour = maskColour;

        private void SetDelay(long delayInMillis) => _delayInMillis = delayInMillis;

        private void SetFadeDuration(long fadeDurationInMillis) => _fadeDurationInMillis = fadeDurationInMillis;

        private void SetTargetTouchable(bool targetTouchable) => _targetTouchable = targetTouchable;

        private void SetDismissOnTargetTouch(bool dismissOnTargetTouch) => _dismissOnTargetTouch = dismissOnTargetTouch;

        private void SetUseFadeAnimation(bool useFadeAnimation) => _useFadeAnimation = useFadeAnimation;

        public void AddShowcaseListener(IShowcaseListener showcaseListener) => _listeners?.Add(showcaseListener);

        public void RemoveShowcaseListener(IShowcaseListener showcaseListener) {

            if (_listeners?.Contains(showcaseListener) == true)
                _listeners.Remove(showcaseListener);

        }

        public void SetDetachedListener(IDetachedListener detachedListener) => _detachedListener = detachedListener;

        public void SetShape(IShape shape) => _shape = shape;

        public void SetAnimationFactory(IAnimationFactory animationFactory) => _animationFactory = animationFactory;

        /**
         * Set properties based on a config object
         *
         * @param config
         */
        public void SetConfig(ShowcaseConfig config) {
            SetDelay(config.Delay);
            SetFadeDuration(config.FadeDuration);
            SetContentTextColor(config.ContentTextColor);
            SetDismissTextColor(config.DismissTextColor);
            SetDismissStyle(config.DismissTextStyle);

            SetMaskColour(config.MaskColor);
            SetShape(config.Shape);
            SetShapePadding(config.ShapePadding);

            SetRenderOverNavigationBar(config.RenderOverNavigationBar);
        }

        private void UpdateDismissButton() {
            // hide or show button
            if (_dismissButton != null) 
                _dismissButton.Visibility = TextUtils.IsEmpty(_dismissButton.Text) ? ViewStates.Gone : ViewStates.Visible;
            
        }

        public bool HasFired() => _prefsManager.HasFired();

        /**
         * BUILDER CLASS
         * Gives us a builder utility class with a fluent API for eaily configuring showcase views
         */
        public class Builder {

            private bool _fullWidth;
            private ShapeType _shapeType = ShapeType.Circle;

            private readonly MaterialShowcaseView _showcaseView;

            private readonly Activity _activity;

            public Builder(Activity activity) {
                _activity = activity;

                _showcaseView = new MaterialShowcaseView(activity);
            }

            /**
             * Set the title text shown on the ShowcaseView.
             */
            public Builder SetTarget(View target) {
                _showcaseView.SetTarget(new ViewTarget(target));
                return this;
            }

            /**
             * Set the title text shown on the ShowcaseView.
             */
            public Builder SetDismissText(int resId) => SetDismissText(_activity.GetString(resId));

            public Builder SetDismissText(string dismissText) {
                _showcaseView.SetDismissText(dismissText);
                return this;
            }

            public Builder SetDismissStyle(Typeface dismissStyle) {
                _showcaseView.SetDismissStyle(dismissStyle);
                return this;
            }

            /**
             * Set the content text shown on the ShowcaseView.
             */
            public Builder SetContentText(int resId) => SetContentText(_activity.GetString(resId));

            /**
             * Set the descriptive text shown on the ShowcaseView.
             */
            public Builder SetContentText(string text) {
                _showcaseView.SetContentText(text);
                return this;
            }

            /**
             * Set the title text shown on the ShowcaseView.
             */
            public Builder SetTitleText(int resId) => SetTitleText(_activity.GetString(resId));

            /**
             * Set the descriptive text shown on the ShowcaseView as the title.
             */
            public Builder SetTitleText(string text) {
                _showcaseView.SetTitleText(text);
                return this;
            }

            /**
             * Set whether or not the target view can be touched while the showcase is visible.
             *
             * False by default.
             */
            public Builder SetTargetTouchable(bool targetTouchable) {
                _showcaseView.SetTargetTouchable(targetTouchable);
                return this;
            }

            /**
             * Set whether or not the showcase should dismiss when the target is touched.
             *
             * True by default.
             */
            public Builder SetDismissOnTargetTouch(bool dismissOnTargetTouch) {
                _showcaseView.SetDismissOnTargetTouch(dismissOnTargetTouch);
                return this;
            }

            public Builder SetDismissOnTouch(bool dismissOnTouch) {
                _showcaseView.SetDismissOnTouch(dismissOnTouch);
                return this;
            }

            public Builder SetMaskColour(Color maskColour) {
                _showcaseView.SetMaskColour(maskColour);
                return this;
            }

            public Builder SetTitleTextColor(Color textColour) {
                _showcaseView.SetTitleTextColor(textColour);
                return this;
            }

            public Builder SetContentTextColor(Color textColour) {
                _showcaseView.SetContentTextColor(textColour);
                return this;
            }

            public Builder SetDismissTextColor(Color textColour) {
                _showcaseView.SetDismissTextColor(textColour);
                return this;
            }

            public Builder SetDelay(int delayInMillis) {
                _showcaseView.SetDelay(delayInMillis);
                return this;
            }

            public Builder SetFadeDuration(int fadeDurationInMillis) {
                _showcaseView.SetFadeDuration(fadeDurationInMillis);
                return this;
            }

            public Builder SetListener(IShowcaseListener listener) {
                _showcaseView.AddShowcaseListener(listener);
                return this;
            }

            public Builder SingleUse(string showcaseId) {
                _showcaseView.SingleUse(showcaseId);
                return this;
            }

            public Builder SetShape(IShape shape) {
                _showcaseView.SetShape(shape);
                return this;
            }

            public Builder WithCircleShape() {
                _shapeType = ShapeType.Circle;
                return this;
            }

            public Builder WithoutShape() {
                _shapeType = ShapeType.NoShape;
                return this;
            }

            public Builder SetShapePadding(int padding) {
                _showcaseView.SetShapePadding(padding);
                return this;
            }

            public Builder WithRectangleShape() => WithRectangleShape(false);

            public Builder WithRectangleShape(bool fullWidth) {
                _shapeType = ShapeType.Rectangle;
                _fullWidth = fullWidth;
                return this;
            }

            public Builder RenderOverNavigationBar() {
                // Note: This only has an effect in Lollipop or above.
                _showcaseView.SetRenderOverNavigationBar(true);
                return this;
            }

            public Builder UseFadeAnimation() {
                _showcaseView.SetUseFadeAnimation(true);
                return this;
            }

            public MaterialShowcaseView Build() {
                if (_showcaseView._shape == null) {
                    switch (_shapeType) {
                        case ShapeType.Rectangle: {
                                _showcaseView.SetShape(new RectangleShape(_showcaseView._target.Bounds, _fullWidth));
                                break;
                            }
                        case ShapeType.Circle: {
                                _showcaseView.SetShape(new CircleShape(_showcaseView._target));
                                break;
                            }
                        case ShapeType.NoShape: {
                                _showcaseView.SetShape(new NoShape());
                                break;
                            }
                        default:
                            throw new IllegalArgumentException("Unsupported shape type: " + _shapeType);
                    }
                }

                if (_showcaseView._animationFactory != null) return _showcaseView;
                // create our animation factory
                if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && !_showcaseView._useFadeAnimation) {
                    _showcaseView.SetAnimationFactory(new CircularRevealAnimationFactory());
                } else {
                    _showcaseView.SetAnimationFactory(new FadeAnimationFactory());
                }

                return _showcaseView;
            }

            public MaterialShowcaseView Show() {
                Build().Show(_activity);
                return _showcaseView;
            }

        }

        private void SingleUse(string showcaseId) {
            _singleUse = true;
            _prefsManager = new PrefsManager(Context, showcaseId);
        }

        public void RemoveFromWindow() {
            if (Parent is ViewGroup parent) 
                parent.RemoveView(this);
            

            if (_bitmap != null) {
                _bitmap.Recycle();
                _bitmap = null;
            }

            _eraser = null;
            _animationFactory = null;
            _canvas = null;
            _handler = null;

            if ((int)Build.VERSION.SdkInt < 16) {
                ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
            } else {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
            }
            //mLayoutListener = null;

            _prefsManager?.Close();

            _prefsManager = null;


        }


        /**
         * Reveal the showcaseview. Returns a bool telling us whether we actually did show anything
         *
         * @param activity
         * @return
         */
        public bool Show(Activity activity) {

            /**
             * if we're in single use mode and have already shot our bolt then do nothing
             */
            if (_singleUse) {
                if (_prefsManager.HasFired())
                    return false;

                _prefsManager.SetFired();

            }

            ((ViewGroup)activity.Window.DecorView).AddView(this);

            SetShouldRender(true);

            _handler = new Handler();

            _handler.PostDelayed(new Runnable(() => {

                if (_shouldAnimate) {
                    AnimateIn();
                } else {
                    Visibility = ViewStates.Visible;
                    NotifyOnDisplayed();
                }
            }), _delayInMillis);

            UpdateDismissButton();

            return true;
        }


        public void Hide() {

            /**
             * This flag is used to indicate to onDetachedFromWindow that the showcase view was dismissed purposefully (by the user or programmatically)
             */
            _wasDismissed = true;

            if (_shouldAnimate) {
                AnimateOut();
            } else {
                RemoveFromWindow();
            }
        }

        public void AnimateIn() {
            Visibility = ViewStates.Invisible;

            _animationFactory.AnimateInView(this, _target.Point, _fadeDurationInMillis, () => {
                Visibility = ViewStates.Visible;
                NotifyOnDisplayed();
            });
        }

        public void AnimateOut() {

            _animationFactory.AnimateOutView(this, _target.Point, _fadeDurationInMillis, () => {
                Visibility = ViewStates.Invisible;
                RemoveFromWindow();
            });
        }

        public void ResetSingleUse() {
            if (_singleUse) _prefsManager?.ResetShowcase();
        }

        /**
         * Static helper method for resetting single use flag
         *
         * @param context
         * @param showcaseID
         */
        public static void ResetSingleUse(Context context, string showcaseId) 
            => PrefsManager.ResetShowcase(context, showcaseId);

        /**
         * Static helper method for resetting all single use flags
         *
         * @param context
         */
        public static void ResetAll(Context context) => PrefsManager.ResetAll(context);

        public static int GetSoftButtonsBarSizePort(Activity activity) {
            // getRealMetrics is only available with API 17 and +
            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1) return 0;
            var metrics = new DisplayMetrics();
            activity.WindowManager.DefaultDisplay.GetMetrics(metrics);
            var usableHeight = metrics.HeightPixels;
            activity.WindowManager.DefaultDisplay.GetRealMetrics(metrics);
            var realHeight = metrics.HeightPixels;
            if (realHeight > usableHeight)
                return realHeight - usableHeight;
            return 0;
        }

        private void SetRenderOverNavigationBar(bool renderOverNav) {
            _renderOverNav = renderOverNav;
        }

        public void OnGlobalLayout() {
            SetTarget(_target);
        }
    }
}
