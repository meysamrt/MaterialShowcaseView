using Android.Content;

namespace MaterialShowcaseViewCore {
    public enum Sequence {
        NeverStarted,
        Finished=-1
    }
    public class PrefsManager {
        
        private const string PrefsName = "material_showcaseview_prefs";
        private const string Status = "status_";
        private readonly string _showcaseId;
        private Context _context;

        public PrefsManager(Context context, string showcaseId) {
            _context = context;
            _showcaseId = showcaseId;
        }


        /***
         * METHODS FOR INDIVIDUAL SHOWCASE VIEWS
         */
        public bool HasFired() => GetSequenceStatus() == Sequence.Finished;
            

        public void SetFired() =>SetSequenceStatus(Sequence.Finished);
        

        /***
         * METHODS FOR SHOWCASE SEQUENCES
         */
        public Sequence GetSequenceStatus() {
            return (Sequence)_context
                .GetSharedPreferences(PrefsName, FileCreationMode.Private)
                .GetInt(Status + _showcaseId,(int) Sequence.NeverStarted);

        }

        public void SetSequenceStatus(Sequence status) {
            var @internal = _context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
            @internal.Edit().PutInt(Status + _showcaseId, (int)status).Apply();
        }


        public void ResetShowcase() {
            ResetShowcase(_context, _showcaseId);
        }

        public static void ResetShowcase(Context context, string showcaseId) {
            var @internal = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
            @internal.Edit().PutInt(Status + showcaseId, (int)Sequence.NeverStarted).Apply();
        }

        public static void ResetAll(Context context) {
            var @internal = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
            @internal.Edit().Clear().Apply();
        }

        public void Close() {
            _context = null;
        }
    }
}