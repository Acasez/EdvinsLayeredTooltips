using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyword", menuName = "ScriptableObjects/Keyword", order = 1)]
public class KeywordScriptableObject : ScriptableObject
{
	public string keywordName;
	public List<string> wordings;
	public string icon;
	public string tooltipText;
}
