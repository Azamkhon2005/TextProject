using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace FileAnalysisService.Application.Services
{
    public static class TextAnalyzer
    {
        public static (int Paragraphs, int Words, int Characters) Analyze(string textContent)
        {
            if (string.IsNullOrEmpty(textContent))
            {
                return (0, 0, 0);
            }

            int charCount = textContent.Length;

            string[] paragraphs = Regex.Split(textContent.Trim(), @"(\r\n|\n){2,}")
                                       .Where(p => !string.IsNullOrWhiteSpace(p))
                                       .ToArray();
            int paragraphCount = paragraphs.Length;

            if (paragraphCount == 0 && !string.IsNullOrWhiteSpace(textContent))
            {
                paragraphCount = 1;
            }

            char[] wordDelimiters = new char[] { ' ', '\r', '\n', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\'' };
            string[] words = textContent.Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries);
            int wordCount = words.Length;

            return (paragraphCount, wordCount, charCount);
        }
    }
}