using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyword", menuName = "ScriptableObjects/Keyword", order = 1)]
public class KeywordScriptableObject : ScriptableObject
{
    [Tooltip("The name for this keyword")]
    public string keywordName;
    [Tooltip("The words that can create tooltips for this in texts and tooltips")]
    public List<string> wordings;
    [Tooltip("The icon for keyword. Needs to be exact same as the name in the sprite sheet to work. If left empty won't create an icon")]
    public string icon;
	[Tooltip("The tooltip that appears when you hover over this")]
    [TextArea] public string tooltipText;
}
