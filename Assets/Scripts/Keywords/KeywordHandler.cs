using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static KeywordHandler;

public class KeywordHandler : MonoBehaviour
{
	[SerializeField] public List<KeywordScriptableObject> keywords;
    [SerializeField] public List<Wording> allWordings;
    [SerializeField] public List<Wording> activeKeywords;

    [Serializable]
    public class Wording
    {
		public string wording;
		public KeywordScriptableObject keyword;
    }

    private void Start()
    {
        MakeWordingList();
    }

    public void MakeWordingList()
	{
		allWordings.Clear();
		Debug.Log("Creating Wording List");
		for (int keywordIndex = 0; keywordIndex < keywords.Count; keywordIndex++)
		{
			KeywordScriptableObject currentKeyword = keywords[keywordIndex];
            Debug.Log("Keyword " + keywordIndex + " name: " + currentKeyword.keywordName);
			for (int wordIndex = 0; wordIndex < currentKeyword.wordings.Count; wordIndex++)
			{
				Wording newWording = new Wording();
				newWording.wording = currentKeyword.wordings[wordIndex];
				newWording.keyword = currentKeyword;
				allWordings.Add(newWording);
            }
		}

        // Sort allWordings by the number of spaces in the wording (descending).
        // Use 0 for null wording strings.
        allWordings = allWordings
            .OrderByDescending(w => (w?.wording?.Count(c => c == ' ')) ?? 0)
            .ToList();
    }

	public void AddKeywordsToText(string text, TextMeshProUGUI textField, bool createTooltips)
	{
		if (keywords == null || keywords.Count == 0)
		{
			textField.text = text;
			return;
		}

		var matchedKeywords = new HashSet<KeywordScriptableObject>();
		var segments = Regex.Split(text, "(<[^>]+>)")
			.Select((s, i) => (text: s, isTag: i % 2 != 0))
			.ToList();
		var processedSegments = new List<string>();
		//Debug.Log("Prossecing segments. There is " + segments.Count + " segments");
		foreach (var (segment, isTag) in segments)
		{
			if (isTag)
			{
				processedSegments.Add(segment);
				continue;
			}

			string processed = segment;
            //Debug.Log("Checking segment " + segment + ".<-> There are " + keywords.Count + " keywords.");
            for (int keywordIndex = 0; keywordIndex < keywords.Count; keywordIndex++)
			{
				KeywordScriptableObject currentKeyword = keywords[keywordIndex];
                //Debug.Log("Current keyword " + currentKeyword.keyword);
                if (matchedKeywords.Contains(currentKeyword) ||
					currentKeyword.wordings == null ||
					currentKeyword.wordings.Count == 0)
					continue;

				// Sort wordings by length (longest first) to prioritize longer matches
				var sortedWordings = currentKeyword.wordings.OrderByDescending(w => w.Length).ToList();
				string pattern = @"\b(" +
					string.Join("|", sortedWordings.Select(w => Regex.Escape(w))) +
					@")\b";

				// Only process the first match for this keyword
				var match = Regex.Match(processed, pattern, RegexOptions.IgnoreCase);

				if (match.Success)
				{
					string matchedText = match.Groups[1].Value;
					string linkId = currentKeyword.keywordName; // Use the keyword

					//Debug.Log("Added keyword " + currentKeyword.keyword + " to matchedKeywords list");
					matchedKeywords.Add(currentKeyword);

					string icon = !string.IsNullOrWhiteSpace(currentKeyword.icon)
						? $"<sprite name=\"{currentKeyword.icon}\">"
						: "";

					// Conditionally wrap the matched text in <link> tags
					if (createTooltips)
					{
						processed = processed.Substring(0, match.Index) +
									$"<link=\"{linkId}\"><u>{matchedText}</u></link>{icon}" +
									processed.Substring(match.Index + match.Length);
					}
					else
					{
						processed = processed.Substring(0, match.Index) +
									$"<u>{matchedText}</u>{icon}" +
									processed.Substring(match.Index + match.Length);
					}
				}
			}

			processedSegments.Add(processed);
		}

		textField.text = string.Concat(processedSegments);
	}

    public void AddKeywordsToTextFromWordingList(string text, TextMeshProUGUI textField, bool createTooltips)
    {
        if (allWordings.Count == 0)
        {
            textField.text = text;
            return;
        }

        var matchedKeywords = new HashSet<KeywordScriptableObject>();
        var segments = Regex.Split(text, "(<[^>]+>)")
            .Select((s, i) => (text: s, isTag: i % 2 != 0))
            .ToList();
        var processedSegments = new List<string>();

        foreach (var (segment, isTag) in segments)
        {
            if (isTag)
            {
                processedSegments.Add(segment);
                continue;
            }

            string processed = segment;
            string originalSegment = segment; // Keep the original for matching

            foreach (var wording in allWordings)
            {
                if (matchedKeywords.Contains(wording.keyword))
                    continue;

                string pattern = $@"\b({Regex.Escape(wording.wording)})\b";
                var match = Regex.Match(originalSegment, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string matchedText = match.Groups[1].Value;
                    string linkId = wording.keyword.keywordName;
                    matchedKeywords.Add(wording.keyword);

                    string icon = !string.IsNullOrWhiteSpace(wording.keyword.icon)
                        ? $"<sprite name=\"{wording.keyword.icon}\">"
                        : "";

                    if (createTooltips)
                    {
                        processed = processed.Substring(0, match.Index) +
                                    $"<link=\"{linkId}\"><u>{matchedText}</u></link>{icon}" +
                                    processed.Substring(match.Index + match.Length);
                    }
                    else
                    {
                        processed = processed.Substring(0, match.Index) +
                                    $"<u>{matchedText}</u>{icon}" +
                                    processed.Substring(match.Index + match.Length);
                    }
                }
            }

            processedSegments.Add(processed);
        }

        textField.text = string.Concat(processedSegments);
    }


}
