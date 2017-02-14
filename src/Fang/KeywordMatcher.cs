using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fang
{
    public class KeywordMatcher
    {
        public KeywordMatcher(string keyword)
        {
            this.Keyword = keyword;
            this._otherKeywordMatcherList = new List<KeywordMatcher>();
            this._regexList = new List<Regex>();

            this.BuildRegex();
        }

        List<KeywordMatcher> _otherKeywordMatcherList;
        List<Regex> _regexList;
        Regex _completelyMatchRegex;

        private string Keyword { set; get; }

        public bool IsMatch(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            if (!this.IsMatchSelf(input) && !this.IsMatchOther(input))
            {
                return false;
            }
            
            return true;
        }

        private bool IsMatchSelf(string input)
        {
            foreach (Regex regex in this._regexList)
            {
                if (!regex.IsMatch(input))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsMatchOther(string input)
        {
            foreach (KeywordMatcher keywordMatcher in this._otherKeywordMatcherList)
            {
                if (keywordMatcher.IsMatch(input))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCompletelyMatch(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            return this._completelyMatchRegex.IsMatch(input);
        }

        private string CleanKeywords(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return keyword;
            }

            Regex regex = new Regex("\\s+");
            keyword = regex.Replace(keyword, " ");

            Regex regex1 = new Regex("[，,]\\s*");
            keyword = regex1.Replace(keyword, ",");

            return keyword.Trim();
        }

        private void BuildRegex()
        {
            string keyword = this.CleanKeywords(this.Keyword);
            List<string> keywords = keyword.Split(',').ToList();
            if (keywords != null && keywords.Count > 0)
            {
                this.BuildRegexList(keywords[0]);

                for (int i = 1; i < keywords.Count; i++)
                {
                    this.BuildOtherKeywordMatcherList(keywords[i]);
                }
            }

            this.BuildCompletelyMatchRegex();
        }

        private void BuildRegexList(string keyword)
        {
            List<string> keywords = keyword.Split(' ').ToList();
            if (keywords != null)
            {
                this._regexList = keywords.Select(x => new Regex(RegexHelper.Escape(x), RegexOptions.IgnoreCase)).ToList();
            }
        }

        private void BuildOtherKeywordMatcherList(string keyword)
        {
            this._otherKeywordMatcherList.Add(new KeywordMatcher(keyword));
        }

        private void BuildCompletelyMatchRegex()
        {
            this._completelyMatchRegex = new Regex(RegexHelper.Escape(this.Keyword));
        }
    }
}
