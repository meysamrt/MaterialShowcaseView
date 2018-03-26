using System;
using Android.Animation;
using Android.Annotation;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;

namespace MaterialShowcaseViewCore {
    public class CircularRevealAnimationFactory : IAnimationFactory {
        

        private readonly AccelerateDecelerateInterpolator _interpolator;

        public CircularRevealAnimationFactory() {
            _interpolator = new AccelerateDecelerateInterpolator();
        }

        [TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
        public void AnimateInView(View target, Point point, long duration, Action onStartListener) {
            var animator = ViewAnimationUtils.CreateCircularReveal(target, point.X, point.Y, 0,
                    target.Width > target.Height ? target.Width : target.Height);

            animator.SetDuration(duration);
            animator.AnimationStart += (s, e) => onStartListener();

            animator.Start();
        }



        [TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
        public void AnimateOutView(View target, Point point, long duration, Action onEndListener) {
            var animator = ViewAnimationUtils.CreateCircularReveal(target, point.X, point.Y,
                    target.Width > target.Height ? target.Width : target.Height, 0);
            animator.SetDuration(duration);
            animator.AnimationEnd += (s, e) => onEndListener();


            animator.Start();
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