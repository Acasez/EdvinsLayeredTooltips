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
        List<KeywordScriptableObject> activeKeywords = new List<KeywordScriptableObject>();

        for (int wordingIndex = 0; wordingIndex < allWordings.Count; wordingIndex++)
        {
            Wording currentWording = allWordings[wordingIndex];
            Debug.Log("Current wording " + currentWording.wording + " with keyword " + currentWording.keyword.keywordName);
            if (activeKeywords.Contains(currentWording.keyword))
            {
                Debug.Log("Keyword already in tooltip");
                continue;
            }
            //Split Up text into segments, ingore the parts in <> brackets
            //Go through those parts and pattern match

            var segments = Regex.Split(text, "(<[^>]+>)")
            .Select((s, i) => (text: s, isTag: i % 2 != 0))
            .ToList();
            var processedSegments = new List<string>();
            Debug.Log("Prossecing segments. There is " + segments.Count + " segments");
            foreach (var (segment, isInBrackets) in segments)
            {
                string currentSegment = segment;
                Debug.Log("Prossecing segment: " + currentSegment);
                if (isInBrackets)
                {
                    Debug.Log("Segment: " + currentSegment + " is already in <> brackets, ignoring");
                    processedSegments.Add(currentSegment); //Add part to bracket, skip checking it
                    continue;
                }

                string pattern = $@"\b({Regex.Escape(currentWording.wording)})\b";
                var match = Regex.Match(currentSegment, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string matchedText = match.Groups[1].Value;
                    string linkId = currentWording.keyword.keywordName;

                    Debug.Log("Added keyword " + currentWording.keyword.keywordName + " to activeKeywords list");
                    activeKeywords.Add(currentWording.keyword);

                    string icon = !string.IsNullOrWhiteSpace(currentWording.keyword.icon)
                        ? $"<sprite name=\"{currentWording.keyword.icon}\">"
                        : "";

                    // Conditionally wrap the matched text in <link> tags
                    if (createTooltips)
                    {
                        currentSegment = currentSegment.Substring(0, match.Index) + $"<link=\"{linkId}\"><u>{matchedText}</u></link>{icon}" + currentSegment.Substring(match.Index + match.Length);
                    }
                    else
                    {
                        currentSegment = currentSegment.Substring(0, match.Index) + $"<u>{matchedText}</u>{icon}" + currentSegment.Substring(match.Index + match.Length);
                    }
                }
                processedSegments.Add(currentSegment);
            }
            text = string.Concat(processedSegments);
            Debug.Log("Text is now " + text + " from segments");
        }

        textField.text = text;
    }

}
