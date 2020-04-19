using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Stopgap.Markup {
    public class Tokenizer {

        public readonly List<Rule> rules = new List<Rule>();

        public List<Token> Tokenize(string input) {
            var res = new List<Token>();

            while (!string.IsNullOrWhiteSpace(input)) {
                Token tok = getNextToken(input);
                
                if (tok == null)
                    throw new Exception("Tokenizer error");

                res.Add(tok);
                input = input.Remove(0, tok.value.Length);
            }

            return res;
        }

        private Token getNextToken(string input) {
            foreach (var rule in rules) {
                if (rule.Match(input, out Token token)) {
                    return token;
                }
            }
            return null;
        }


        public class Rule {
            public readonly string Name;
            private readonly Regex regex;

            public Rule(string name, string pattern) {
                Name = name; regex = new Regex($"^({pattern})");
            }

            public bool Match(string src, out Token token) {
                var m = regex.Match(src);
                if (m.Success) {
                    token = new Token(this, m.Value);
                    return true;
                }
                token = null;
                return false;
            }
        }

        public class Token {
            public readonly Rule rule;
            public readonly string value;

            public Token(Rule rule, string value) {
                this.rule = rule; this.value = value;
            }
        }
    }
}
