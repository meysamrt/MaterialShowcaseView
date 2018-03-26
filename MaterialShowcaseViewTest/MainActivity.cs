using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using MaterialShowcaseViewCore;

namespace MaterialShowcaseViewTest {
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : AppCompatActivity,View.IOnClickListener {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Button button = FindViewById<Button>(Resource.Id.btn_simple_example);
            button.SetOnClickListener(this);
            button = FindViewById<Button>(Resource.Id.btn_custom_example);
            button.SetOnClickListener(this);
            button = FindViewById<Button>(Resource.Id.btn_sequence_example);
            button.SetOnClickListener(this);
            button = FindViewById<Button>(Resource.Id.btn_reset_all);
            button.SetOnClickListener(this);
        }

        public void OnClick(View v) {
            Intent intent = null;

            switch (v.Id) {
                case Resource.Id.btn_simple_example:
                    intent = new Intent(this,typeof(SimpleSingleExample));
                    break;

                case Resource.Id.btn_custom_example:
                    intent = new Intent(this, typeof(CustomExample));
                    break;

                case Resource.Id.btn_sequence_example:
                    intent = new Intent(this, typeof(SequenceExample));
                    break;

                case Resource.Id.btn_reset_all:
                    MaterialShowcaseView.ResetAll(this);
                    Toast.MakeText(this, "All Showcases reset", ToastLength.Long).Show();
                    break;
            }

            if(intent!=null){
                StartActivity(intent);
            }
        }
    }
}

