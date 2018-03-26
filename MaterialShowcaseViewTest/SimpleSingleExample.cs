using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using MaterialShowcaseViewCore;

namespace MaterialShowcaseViewTest {
    [Activity(Label = "@string/title_activity_simple_single_example")]
    public class SimpleSingleExample : AppCompatActivity, View.IOnClickListener {
        private Button _buttonShow;
        private Button _buttonReset;

        private const string ShowcaseId = "simple example";
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_simple_single_example);
            _buttonShow = FindViewById<Button>(Resource.Id.btn_show);
            _buttonShow.SetOnClickListener(this);

            _buttonReset = FindViewById<Button>(Resource.Id.btn_reset);
            _buttonReset.SetOnClickListener(this);

            PresentShowcaseView(1000); // one second delay
        }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.btn_show:
                    PresentShowcaseView(0);
                    break;
                case Resource.Id.btn_reset:
                    MaterialShowcaseView.ResetSingleUse(this, ShowcaseId);
                    Toast.MakeText(this, "Showcase reset", ToastLength.Long).Show();
                    break;
            }
        }
        private void PresentShowcaseView(int withDelay) {
            new MaterialShowcaseView.Builder(this)
                .SetTarget(_buttonShow)
                .SetTitleText("Hello")
                .SetDismissText("GOT IT")
                .SetContentText("This is some amazing feature you should know about")
                .SetDelay(withDelay) // optional but starting animations immediately in onCreate can make them choppy
                .SingleUse(ShowcaseId) // provide a unique ID used to ensure it is only shown once
                //                .useFadeAnimation() // remove comment if you want to use fade animations for Lollipop & up
                .Show();
        }
    }
}