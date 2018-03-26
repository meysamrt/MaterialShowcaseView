using Android.Graphics;
using MaterialShowcaseViewCore.Shape;

namespace MaterialShowcaseViewCore {
    public class ShowcaseConfig {
        public static readonly Color DefaultMaskColour = Color.ParseColor("#dd335075");
        public const long DefaultFadeTime = 300;
        public const long DefaultDelay = 0;
        public static readonly IShape DefaultShape = new CircleShape();
        public const int DefaultShapePadding = 10;

        public long Delay { get; set; } = DefaultDelay;
        public Color MaskColor { get; set; } = DefaultMaskColour;
        public Color ContentTextColor { get; set; } = Color.ParseColor("#ffffff");
        public Color DismissTextColor { get; set; } = Color.ParseColor("#ffffff");
        public Typeface DismissTextStyle { get; set; } = Typeface.DefaultBold;
        public long FadeDuration { get; set; } = DefaultFadeTime;
        public IShape Shape { get; set; } = DefaultShape;
        public int ShapePadding { get; set; } = DefaultShapePadding;
        public bool RenderOverNavigationBar { get; set; }
    }
}