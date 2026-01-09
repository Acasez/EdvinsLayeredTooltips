using System.Collections.Generic;
using static KeywordHandler;

public static class HighlightStyles
{
    public static readonly HighlightStyle Underline = new("Underline", "<u>", "</u>", HighlightStyleEnum.Underline);
    public static readonly HighlightStyle Bold = new("Bold", "<b>", "</b>", HighlightStyleEnum.Bold);
    public static readonly HighlightStyle Italic = new("Italic", "<i>", "</i>", HighlightStyleEnum.Italic);
    public static readonly HighlightStyle UnderlineBold = new("UnderlineBold", "<u><b>", "</u></b>", HighlightStyleEnum.UnderlineBold);
    public static readonly HighlightStyle CK3Style = new("CK3Style", "<color=#67F9FF><b>", "</color></b>", HighlightStyleEnum.CK3Style);

    // Dictionary to map enum values to styles
    public static readonly Dictionary<HighlightStyleEnum, HighlightStyle> StyleMap = new()
    {
        { HighlightStyleEnum.Underline, Underline },
        { HighlightStyleEnum.Bold, Bold },
        { HighlightStyleEnum.Italic, Italic },
        { HighlightStyleEnum.UnderlineBold, UnderlineBold },
        { HighlightStyleEnum.CK3Style, CK3Style },
    };

    public class HighlightStyle
    {
        public string name;
        public string openingText;
        public string closingText;
        public HighlightStyleEnum emum;

        public HighlightStyle(string v1, string v2, string v3, HighlightStyleEnum v4)
        {
            name = v1;
            openingText = v2;
            closingText = v3;
            emum = v4;
        }
    }
}
