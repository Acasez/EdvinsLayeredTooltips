using UnityEngine;

namespace LayeredTooltips
{
    [CreateAssetMenu(fileName = "NewKeyword", menuName = "ScriptableObjects/TooltipWindowStyle", order = 1)]
    public class TooltipWindowStyle : ScriptableObject
    {
        [Header("Name")]
        [Tooltip("Name of the style")]
        public string styleName;

        [Header("Window Colors")]
        [Tooltip("Color of the background of the tooltip window")]
        public Color backgroundColor;
        [Tooltip("Color border of the tooltip")]
        public Color defaultBorderColor;
        [Tooltip("Color border of the tooltip when locked.")]
        public Color lockedBorderColor;

        [Header("Text")]
        [Tooltip("Color of the text")]
        public Color textColor;
        [Tooltip("Should the title of keywords be shown above the text")]
        public bool showTitle = true;
        [Tooltip("How large, in % should the keyword title text be")]
        public int titleTextSize = 125;
        [Tooltip("How large, in % should the keyword type text be")]
        public int typeTextSize = 110;
        [Tooltip("If title is displayed, should the icon be next to it?")]
        public bool keywordIconNextToTitle = true;

        [Header("Keywords")]
        [Tooltip("Color of the keywords")]
        public Color keywordColor;
        public bool bold;
        public bool italic;
        public bool underline;

        public string openingText;
        public string closingText;

        private void OnEnable()
        {
            openingText = "";
            closingText = "";
            if (keywordColor != textColor)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(keywordColor);
                openingText += $"<color=#{hexColor}>";
                closingText += "</color>";
            }
            if (bold)
            {
                openingText += "<b>";
                closingText += "</b>";
            }
            if (italic)
            {
                openingText += "<i>";
                closingText += "</i>";
            }
            if (underline)
            {
                openingText += "<u>";
                closingText += "</u>";
            }
        }
    }
}