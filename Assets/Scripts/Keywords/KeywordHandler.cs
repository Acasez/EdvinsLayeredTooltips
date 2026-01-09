using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class KeywordHandler : MonoBehaviour
{
    public enum HighlightStyleEnum
    {
        Underline,
        Bold,
        Italic,
    }

    [Header("Lists")]
    [Tooltip("The keywords used in the the game. Needs to be updated when you add new keywords.")]
    [SerializeField] public List<KeywordScriptableObject> keywords;
    [SerializeField] public List<Wording> allWordings;
    [SerializeField] public List<Wording> activeKeywords;

    [Header("Decoration")]
    [SerializeField] HighlightStyleEnum highlightStyle;
    [SerializeField] HighlightStyle chosenHighlightStyle;
    [SerializeField] List<HighlightStyle> highlightStyles; 

    [Serializable]
    public class Wording
    {
		public string wording;
		public KeywordScriptableObject keyword;
    }
    [Serializable]
    public class HighlightStyle
    {
        public string name;
        public HighlightStyleEnum highlightStyleEnum;
        public string openingText;
        public string closingText;
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

            Debug.Log("Current Text is ");
            var segments = SplitTextIntoSegments(text); //Split up text into segments split by <> brackets
            var processedSegments = new List<string>();

            Debug.Log("Prossecing segments. There are " + segments.Count + " segments");
            foreach (var (segment, isInBrackets) in segments)
            {
                string currentSegment = segment;
                Debug.Log("Prossecing segment: " + currentSegment);
                if (string.IsNullOrWhiteSpace(currentSegment))
                {
                    Debug.Log("Segment: " + currentSegment + " is empty, ignoring");
                    continue;
                }
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
                    Debug.Log("Found Match! " + currentWording.wording + " for keyword " + currentWording.keyword.keywordName);
                    string matchedText = match.Groups[1].Value;
                    string linkId = currentWording.keyword.keywordName;

                    Debug.Log("Added keyword " + currentWording.keyword.keywordName + " to activeKeywords list");
                    activeKeywords.Add(currentWording.keyword);

                    string icon = !string.IsNullOrWhiteSpace(currentWording.keyword.icon) ? $"<sprite name=\"{currentWording.keyword.icon}\">": "";

                    // Conditionally wrap the matched text in <link> tags
                    if (createTooltips)
                    {
                        currentSegment = currentSegment[..match.Index] + $"<link=\"{linkId}\"><u>{matchedText}</u></link>{icon}" + currentSegment[(match.Index + match.Length)..];
                    }
                    else
                    {
                        currentSegment = currentSegment[..match.Index] + $"<u>{matchedText}</u>{icon}" + currentSegment[(match.Index + match.Length)..];
                    }
                }
                processedSegments.Add(currentSegment);
            }
            text = string.Concat(processedSegments);
            Debug.Log("Text is now " + text + " from segments");
        }

        textField.text = text;
    }

    public static List<(string text, bool isInBrackets)> SplitTextIntoSegments(string text)
    {
        var segments = new List<(string, bool)>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return segments;
        }

        int start = 0;
        bool inBrackets = false;
        var stack = new Stack<string>(); // Tracks the type of opening tag

        for (int i = 0; i < text.Length; i++)
        {
            if (text.Substring(i).StartsWith("<u>"))
            {
                // Push current segment if not empty
                if (i > start)
                {
                    segments.Add((text.Substring(start, i - start), inBrackets));
                }
                stack.Push("<u>");
                start = i;
                inBrackets = true;
                i += 2; // Skip past "<u"
            }
            else if (text.Substring(i).StartsWith("</u>"))
            {
                // Pop and check if we're closing the outermost bracket
                if (stack.Count > 0 && stack.Peek() == "<u>")
                {
                    stack.Pop();
                    segments.Add((text.Substring(start, i - start + 4), true)); // Include "</u>"
                    start = i + 4; // Skip past "</u>"
                    inBrackets = (stack.Count > 0);
                }
                i += 3; // Skip past "</u>"
            }
            else if (text[i] == '<')
            {
                // Handle other opening tags (e.g., <b>, <i>, etc.)
                if (i > start)
                {
                    segments.Add((text.Substring(start, i - start), inBrackets));
                }
                stack.Push("<");
                start = i;
                inBrackets = true;
            }
            else if (text[i] == '>' && stack.Count > 0 && stack.Peek() == "<")
            {
                // Handle closing of other tags
                stack.Pop();
                segments.Add((text.Substring(start, i - start + 1), true));
                start = i + 1;
                inBrackets = (stack.Count > 0);
            }
        }

        // Add the remaining text
        if (start < text.Length)
        {
            segments.Add((text.Substring(start), inBrackets));
        }

        return segments;
    }

}
