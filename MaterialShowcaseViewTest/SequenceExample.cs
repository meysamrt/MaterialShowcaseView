using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using MaterialShowcaseViewCore;

namespace MaterialShowcaseViewTest {
    [Activity(Label = "@string/title_activity_sequence_example")]
    public class SequenceExample : AppCompatActivity, View.IOnClickListener {
        private Button _buttonOne;
        private Button _buttonTwo;
        private Button _buttonThree;

        private Button _buttonReset;

        private const string ShowcaseId = "sequence example";

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_sequence_example);
            _buttonOne = FindViewById<Button>(Resource.Id.btn_one);
            _buttonOne.SetOnClickListener(this);

            _buttonTwo = FindViewById<Button>(Resource.Id.btn_two);
            _buttonTwo.SetOnClickListener(this);

            _buttonThree = FindViewById<Button>(Resource.Id.btn_three);
            _buttonThree.SetOnClickListener(this);

            _buttonReset = FindViewById<Button>(Resource.Id.btn_reset);
            _buttonReset.SetOnClickListener(this);

            PresentShowcaseSequence(); // one second delay
        }

        public void OnClick(View v) {
            switch (v.Id) {
                case Resource.Id.btn_one:
                case Resource.Id.btn_two:
                case Resource.Id.btn_three:
                    PresentShowcaseSequence();
                    break;
                case Resource.Id.btn_reset:
                    MaterialShowcaseView.ResetSingleUse(this, ShowcaseId);
                    Toast.MakeText(this, "Showcase reset", ToastLength.Long).Show();
                    break;
            }
        }

        private void PresentShowcaseSequence() {

            var config = new ShowcaseConfig {
                Delay = 500 // half second between each showcase view
            };
            

            var sequence = new MaterialShowcaseSequence(this, ShowcaseId);


            sequence.SetOnItemShownListener((itemView, position) =>
                Toast.MakeText(itemView.Context, "Item #" + position, ToastLength.Long).Show()

            );

            sequence.SetConfig(config);

            sequence.AddSequenceItem(_buttonOne, "This is button one", "GOT IT");

            sequence.AddSequenceItem(
                new MaterialShowcaseView.Builder(this)
                    .SetTarget(_buttonTwo)
                    .SetDismissText("GOT IT")
                    .SetContentText("This is button two")
                    .WithRectangleShape(true)
                    .Build()
            );

            sequence.AddSequenceItem(
                new MaterialShowcaseView.Builder(this)
                    .SetTarget(_buttonThree)
                    .SetDismissText("GOT IT")
                    .SetContentText("This is button three")
                    .WithRectangleShape()
                    .Build()
            );

            sequence.Start();

        }
    }
}