using System;
using Android.Animation;
using Android.Graphics;
using Android.Views;
using Android.Views.Animations;

namespace MaterialShowcaseViewCore {
    public class FadeAnimationFactory : IAnimationFactory {
        private const string Alpha = "alpha";
        private const float Invisible = 0f;
        private const float Visible = 1f;

        private readonly AccelerateDecelerateInterpolator _interpolator;

        public FadeAnimationFactory() {
            _interpolator = new AccelerateDecelerateInterpolator();
        }

        public void AnimateInView(View target, Point point, long duration, Action onStartListener) {
            var oa = ObjectAnimator.OfFloat(target, Alpha, Invisible, Visible);
            oa.SetDuration(duration);
            oa.AnimationStart += (s, e) => onStartListener();

            oa.Start();
        }

        public void AnimateOutView(View target, Point point, long duration, Action onEndListener) {
            var oa = ObjectAnimator.OfFloat(target, Alpha, Invisible);
            oa.SetDuration(duration);
            oa.AnimationEnd += (s, e) => onEndListener();

            oa.Start();
        }

        public void AnimateTargetToPoint(MaterialShowcaseView showcaseView, Point point) {
            var set = new AnimatorSet();
            var xAnimator = ObjectAnimator.OfInt(showcaseView, "showcaseX", point.X);
            var yAnimator = ObjectAnimator.OfInt(showcaseView, "showcaseY", point.Y);
            set.PlayTogether(xAnimator, yAnimator);
            set.SetInterpolator(_interpolator);
            set.Start();
        }
    }
}