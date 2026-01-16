using System.Collections.Generic;
using UnityEngine;
using static KeywordHandler;

[CreateAssetMenu(fileName = "NewKeyword", menuName = "ScriptableObjects/TooltipWindowStyle", order = 1)]
public class TooltipWindowStyle : ScriptableObject
{
    [Tooltip("Name of the style")]
    public string styleName;

    [Tooltip("Bakground Decoration")]
    public Color backgroundColor;

    [Tooltip("Text Style")]
    public Color textColor;

    [Tooltip("Keyword Decoration")]
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
        if (bold) { 
            openingText += "<b>";
            closingText += "</b>";
        }
        if (italic) { 
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
