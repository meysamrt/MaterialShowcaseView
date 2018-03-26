using System;
using Android.Graphics;
using Android.Views;

namespace MaterialShowcaseViewCore {
    public interface IAnimationFactory {
        void AnimateInView(View target, Point point, long duration, Action onStartListener);

        void AnimateOutView(View target, Point point, long duration, Action onEndListener);

        void AnimateTargetToPoint(MaterialShowcaseView showcaseView, Point point);

        
    }
   
}