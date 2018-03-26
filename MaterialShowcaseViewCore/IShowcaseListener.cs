namespace MaterialShowcaseViewCore {
    public interface IShowcaseListener {
        void OnShowcaseDisplayed(MaterialShowcaseView showcaseView);
        void OnShowcaseDismissed(MaterialShowcaseView showcaseView);
    }
}