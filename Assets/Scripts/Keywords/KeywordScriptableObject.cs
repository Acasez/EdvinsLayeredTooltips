using System.Collections.Generic;
using UnityEngine;
using static KeywordHandler;

[CreateAssetMenu(fileName = "NewKeyword", menuName = "ScriptableObjects/Keyword", order = 1)]
public class KeywordScriptableObject : ScriptableObject
{
    [Tooltip("The name for this keyword")]
    public string keywordName;
    [Tooltip("The type of keyword")]
    public KeywordType keywordType;
    [Tooltip("The words that can create tooltips for this in texts and tooltips")]
    public List<string> wordings;
    [Tooltip("The icon for keyword. Needs to be exact same as the name in the sprite sheet to work. If left empty won't create an icon")]
    public string icon;
    [Tooltip("Negative Icons are optional. They will appear if there is a negative number in front of the keyword")]
    public string negativeIcon;
    [Tooltip("The tooltip that appears when you hover over this")]
    [TextArea] public string tooltipText;
    [SerializeField] string textWithTooltips;
    [Tooltip("Only if you want tooltips for this keyword to have a specific TooltipStyle")]
    public TooltipWindowStyle specificTooltipStyle;

    public void StoreTextWithTooltip(string text)
    {
        textWithTooltips = text;
    }

    public string GetTextWithTooltip()
    {
        return textWithTooltips;
    }

    private void OnEnable()
    {
        textWithTooltips = string.Empty;
    }
}
