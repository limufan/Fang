using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fang
{
    public class RegexList: List<Regex>
    {
        public bool IsMatch(string input)
        {
            foreach(Regex regex in this)
            {
                if(regex.IsMatch(input))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
