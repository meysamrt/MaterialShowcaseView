using System;
using System.Collections.Generic;
using Android.App;
using Android.Views;

namespace MaterialShowcaseViewCore {
    public class MaterialShowcaseSequence : IDetachedListener {
        private PrefsManager _prefsManager;
        private readonly Queue<MaterialShowcaseView> _showcaseQueue;
        private bool _singleUse;
        private readonly Activity _activity;
        private ShowcaseConfig _config;
        private Sequence _sequencePosition;

        private Action<MaterialShowcaseView, int> _onItemShownListener;
        private Action<MaterialShowcaseView, int> _onItemDismissedListener;

        public MaterialShowcaseSequence(Activity activity) {
            _activity = activity;
            _showcaseQueue = new Queue<MaterialShowcaseView>();
        }

        public MaterialShowcaseSequence(Activity activity, string sequenceId) : this(activity) {
            SingleUse(sequenceId);
        }

        public MaterialShowcaseSequence AddSequenceItem(View targetView, string content, string dismissText) {
            AddSequenceItem(targetView, "", content, dismissText);
            return this;
        }

        public MaterialShowcaseSequence AddSequenceItem(View targetView, string title, string content, string dismissText) {

            var sequenceItem = new MaterialShowcaseView.Builder(_activity)
                    .SetTarget(targetView)
                    .SetTitleText(title)
                    .SetDismissText(dismissText)
                    .SetContentText(content)
                    .Build();

            if (_config != null)
                sequenceItem.SetConfig(_config);


            _showcaseQueue.Enqueue(sequenceItem);
            return this;
        }

        public MaterialShowcaseSequence AddSequenceItem(MaterialShowcaseView sequenceItem) {
            _showcaseQueue.Enqueue(sequenceItem);
            return this;
        }

        public MaterialShowcaseSequence SingleUse(string sequenceId) {
            _singleUse = true;
            _prefsManager = new PrefsManager(_activity, sequenceId);
            return this;
        }

        public void SetOnItemShownListener(Action<MaterialShowcaseView, int> shownListener) {
            _onItemShownListener = shownListener;
        }

        public void SetOnItemDismissedListener(Action<MaterialShowcaseView, int> dismissListener) {
            _onItemDismissedListener = dismissListener;
        }

        public bool HasFired => _prefsManager.GetSequenceStatus() == Sequence.Finished;


        public void Start() {

            /**
             * Check if we've already shot our bolt and bail out if so         *
             */
            if (_singleUse) {
                if (HasFired)
                    return;


                /**
                 * See if we have started this sequence before, if so then skip to the point we reached before
                 * instead of showing the user everything from the start
                 */
                _sequencePosition = _prefsManager.GetSequenceStatus();

                if (_sequencePosition > 0)
                    for (var i = 0; i < (int)_sequencePosition; i++)
                        _showcaseQueue.Dequeue();


            }


            // do start
            if (_showcaseQueue.Count > 0)
                ShowNextItem();
        }

        private void ShowNextItem() {

            if (_showcaseQueue.Count > 0 && !_activity.IsFinishing) {
                var sequenceItem = _showcaseQueue.Dequeue();
                sequenceItem.SetDetachedListener(this);
                sequenceItem.Show(_activity);
                _onItemShownListener?.Invoke(sequenceItem, (int)_sequencePosition);
            } else
                /**
                 * We've reached the end of the sequence, save the fired state
                 */
                if (_singleUse)
                _prefsManager.SetFired();


        }


        public void OnShowcaseDetached(MaterialShowcaseView showcaseView, bool wasDismissed) {

            showcaseView.SetDetachedListener(null);

            /**
             * We're only interested if the showcase was purposefully dismissed
             */
            if (!wasDismissed) return;
            _onItemDismissedListener?.Invoke(showcaseView, (int)_sequencePosition);

            /**
                     * If so, update the prefsManager so we can potentially resume this sequence in the future
                     */
            if (_prefsManager != null) {
                _sequencePosition++;
                _prefsManager.SetSequenceStatus(_sequencePosition);
            }

            ShowNextItem();
        }

        public void SetConfig(ShowcaseConfig config) {
            _config = config;
        }


    }
}