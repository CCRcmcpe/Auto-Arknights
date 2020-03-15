using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace REVUnit.AutoArknights.Library.Automation_Script
{
    public class Script2
    {
        private class TokenScreen
        {
            public Token[] Tokens { get; set; }
            public int Index { get; set; }

            public bool End => Index == Tokens.Length;

            public TokenScreen(string[] tokens)
            {
                Tokens = tokens;
            }

            public Token Peek()
            {
                return Tokens[Index + 1];
            }

            public Token Next()
            {
                return Tokens[Index++];
            }
        }

        public void Run(string script)
        {
            var trimed = Regex.Replace(script, @"\s+", " ");
            var lines = trimed.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            List<>
            foreach (var line in lines)
            {
                var tokens = line.Split(' ');
                switch (tokens[0])
                {
                    
                }
            }
        }
    }
}