using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using MaterialShowcaseViewCore;

namespace MaterialShowcaseViewTest {
    [Activity(Label = "@string/title_activity_custom_example")]
    public class CustomExample : AppCompatActivity,View.IOnClickListener {
        private Button _buttonShow;
        private Button _buttonReset;

        private const string ShowcaseId = "custom example";
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_custom_example);
            _buttonShow = FindViewById<Button>(Resource.Id.btn_show);
            _buttonShow.SetOnClickListener(this);

            _buttonReset = FindViewById<Button>(Resource.Id.btn_reset);
            _buttonReset.SetOnClickListener(this);

            PresentShowcaseView(1000); // one second delay
        }
        public override bool OnCreateOptionsMenu(IMenu menu) {
            MenuInflater.Inflate(Resource.Menu.activity_custom_example, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override  bool OnOptionsItemSelected(IMenuItem item) {
            if (item.ItemId != Resource.Id.menu_sample_action) return base.OnOptionsItemSelected(item);
            var view = FindViewById(Resource.Id.menu_sample_action);
            new MaterialShowcaseView.Builder(this)
                .SetTarget(view)
                .SetShapePadding(96)
                .SetDismissText("GOT IT")
                .SetContentText("Example of how to setup a MaterialShowcaseView for menu items in action bar.")
                .SetContentTextColor(new Color(ContextCompat.GetColor(this,Resource.Color.green)))
                .SetMaskColour(new Color(ContextCompat.GetColor(this, Resource.Color.purple)))
                .Show();

            return base.OnOptionsItemSelected(item);
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
                .SetContentText("This is some amazing feature you should know about")
                .SetDismissOnTouch(true)
                .SetContentTextColor(new Color(ContextCompat.GetColor(this, Resource.Color.green)))
                .SetMaskColour(new Color(ContextCompat.GetColor(this, Resource.Color.purple)))
                .SetDelay(withDelay) // optional but starting animations immediately in onCreate can make them choppy
                .SingleUse(ShowcaseId) // provide a unique ID used to ensure it is only shown once
                .Show();
        }
    }
}