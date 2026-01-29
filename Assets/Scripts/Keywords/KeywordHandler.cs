using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class KeywordHandler : MonoBehaviour
{
    public enum KeywordType
    {
        None,
        Action,
        Resource,
        Mechanic,
        Condition,
        CardType,
        Location
    }

    [Header("Lists")]
    [Tooltip("The keywords used in the the game. Needs to be updated when you add new keywords.")]
    public List<KeywordScriptableObject> keywords;
    private static List<Wording> _allWordingsCached;
    // Accessor property
    public IReadOnlyList<Wording> AllWordings => _allWordingsCached;

    [Header("References")]
    [SerializeField] TooltipManager tooltipManager;
    TooltipWindowStyle tooltipWindowStyle;

    [Serializable]
    public class Wording
    {
        public string wording;
        public KeywordScriptableObject keyword;
    }

    private void Start()
    {
        MakeWordingList();
        tooltipManager = FindAnyObjectByType<TooltipManager>();
        if (tooltipManager == null)
        {
            Debug.LogWarning("No tooltipManager!");
        }
        tooltipWindowStyle = tooltipManager.GetTooltipDecoration();
    }

    public void MakeWordingList()
    {
        List<Wording> newWordings = new();
        for (int keywordIndex = 0; keywordIndex < keywords.Count; keywordIndex++)
        {
            KeywordScriptableObject currentKeyword = keywords[keywordIndex];
            for (int wordIndex = 0; wordIndex < currentKeyword.wordings.Count; wordIndex++)
            {
                newWordings.Add(new Wording { wording = currentKeyword.wordings[wordIndex], keyword = currentKeyword });
            }
        }

        // Sort by number of spaces (descending)
        _allWordingsCached = newWordings.OrderByDescending(w => w.wording?.Count(c => c == ' ') ?? 0).ToList();
    }

    public string AddKeywordsToText(string text, bool createTooltips, KeywordScriptableObject keyword)
    {
        if (AllWordings.Count == 0)
        {
            Debug.LogWarning("No wordings, you might have missed to add them");
            return text;
        }

        List<KeywordScriptableObject> activeKeywords = new();
        if (keyword != null)
        {
            if (keyword.GetTextWithTooltip() != string.Empty)
            {
                Debug.Log("Tooltip with keyword all setup, using that");
                return keyword.GetTextWithTooltip();
            }
            activeKeywords.Add(keyword);
        }

        for (int wordingIndex = 0; wordingIndex < AllWordings.Count; wordingIndex++)
        {
            Wording currentWording = AllWordings[wordingIndex];
            if (activeKeywords.Contains(currentWording.keyword))
            {
                continue;
            }

            var segments = SplitTextIntoSegments(text); //Split up text into segments split by <> brackets
            var processedSegments = new List<string>();

            //Go through those parts and pattern match
            foreach (var (segment, isInBrackets) in segments)
            {
                string currentSegment = segment;
                if (string.IsNullOrWhiteSpace(currentSegment))
                {
                    continue; //Ignore empty segments
                }
                if (isInBrackets)
                {
                    processedSegments.Add(currentSegment); //Add part to bracket, skip checking it
                    continue;
                }

                string pattern = $@"([+-]?\d+\s+)?\b({Regex.Escape(currentWording.wording)})\b";
                var match = Regex.Match(currentSegment, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string modifier = match.Groups[1].Value; // e.g., "+2 ", "-1 "
                    string matchedText = match.Groups[2].Value; // The keyword itself
                    string linkId = currentWording.keyword.keywordName;
                    activeKeywords.Add(currentWording.keyword);

                    // Check if the modifier is negative
                    bool isNegative = !string.IsNullOrEmpty(modifier) && modifier.Trim().StartsWith("-");

                    string icon = "";
                    if (isNegative && !string.IsNullOrWhiteSpace(currentWording.keyword.negativeIcon))
                    {
                        icon = "<sprite name=\"" + currentWording.keyword.negativeIcon + "\">";
                    }
                    else if (!string.IsNullOrWhiteSpace(currentWording.keyword.icon))
                    {
                        icon = "<sprite name=\"" + currentWording.keyword.icon + "\">";
                    }

                    string beforeMatch = currentSegment[..match.Index];
                    string afterMatch = currentSegment[(match.Index + match.Length)..];

                    if (createTooltips)
                    {
                        currentSegment = beforeMatch + modifier + "<link=\"" + linkId + "\">" + tooltipWindowStyle.openingText + matchedText + tooltipWindowStyle.closingText + "</link>" + icon + afterMatch;
                    }
                    else
                    {
                        currentSegment = beforeMatch + modifier + tooltipWindowStyle.openingText + matchedText + tooltipWindowStyle.closingText + icon + afterMatch;
                    }
                }
                processedSegments.Add(currentSegment);
            }
            text = string.Concat(processedSegments); //Merge Text Together from segments
        }
        if (keyword != null)
        {
            keyword.StoreTextWithTooltip(text);
        }
        return text;
    }

    public List<(string text, bool isInBrackets)> SplitTextIntoSegments(string text)
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
            // Check for the current highlight style's opening tag
            if (text.Substring(i).StartsWith(tooltipWindowStyle.openingText))
            {
                // Push current segment if not empty
                if (i > start)
                {
                    segments.Add((text[start..i], inBrackets));
                }
                stack.Push(tooltipWindowStyle.openingText);
                start = i;
                inBrackets = true;
                i += tooltipWindowStyle.openingText.Length - 1; // Skip past openingText
            }
            else if (text.Substring(i).StartsWith(tooltipWindowStyle.closingText) && stack.Count > 0 && stack.Peek() == tooltipWindowStyle.openingText)
            {
                stack.Pop();
                segments.Add((text.Substring(start, i - start + tooltipWindowStyle.closingText.Length), true));
                start = i + tooltipWindowStyle.closingText.Length;
                inBrackets = (stack.Count > 0);
                i += tooltipWindowStyle.closingText.Length - 1; // Skip past closingText
            }
            // Handle generic < and > tags 
            else if (text[i] == '<' && !text.Substring(i).StartsWith(tooltipWindowStyle.openingText))
            {
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
                stack.Pop();
                segments.Add((text.Substring(start, i - start + 1), true));
                start = i + 1;
                inBrackets = (stack.Count > 0);
            }
        }

        // Add the remaining text
        if (start < text.Length)
        {
            segments.Add((text[start..], inBrackets));
        }

        return segments;
    }
}
