using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace REVUnit.AutoArknights.Library.Automation_Script
{
    public static class Script
    {
        public static void Execute(string script)
        {
            var reconstructed = Reconstruct(script);
            var tokens = Lexing(reconstructed);
            var commands = Parse(tokens);
            foreach (var command in commands)
            {
                command.Execute();
            }
        }

        private class TokenScreen
        {
            public Token[] Tokens { get; set; }
            public int Index { get; set; }

            public bool End => Index == Tokens.Length;

            public TokenScreen(Token[] tokens)
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

        public static string Reconstruct(string code)
        {
            return Regex.Replace(code, @"\s+", " ");
        }
        
        public static Token[] Lexing(string code)
        {
            var result = new List<Token>();
            var tokenStrs = code.Split(' ');
            Token token = tokenStrs.Select(str =>
            {
                var parseResults = new Func<Token?>[]
                {
                    () => Keyword.Parse(str), //Current word is a keyword
                    () => new Identfier(str), //Current word is a identfier
                }.Select(parser => parser.Invoke());
                return parseResults.First(it => it != null);
            }).First()!;
            result.Add(token);
            return result.ToArray();
        }

        public static Command[] Parse(Token[] tokens)
        {
            throw new NotImplementedException();
            var syntaxes = new List<Command>();
            var screen = new TokenScreen(tokens);

            void TokenError()
            {
                throw new Exception($"At token #{screen.Index} : Error");
            }

            while (!screen.End)
            {
                var currentToken = screen.Next();
                var next = screen.Peek();

                Command ParseSyntax()
                {
                    switch (currentToken)
                    {
                        case Click click:
                            if (next is Identfier identfier)
                            {
                                return new ClickCommand {Target = identfier};
                            }
                            break;
                        case If @if:
                            if (next is )
                            {
                                
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }

                syntaxes.Add(ParseSyntax());
            }
            return syntaxes.ToArray();
        }
    }
}