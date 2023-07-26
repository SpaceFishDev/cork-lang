using System.Diagnostics.CodeAnalysis;

namespace Cork
{
    struct Token
    {
        public TokenType Type;
        public int Line;
        public int Column;
        public string Text;

        public override string ToString()
        {
            return $"Token([{Type}]<{Line}, {Column}>: {Text})";
        }

        public Token(int line, int column, string txt, TokenType type)
        {
            Line = line;
            Column = column;
            Text = txt;
            Type = type;
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType().TypeHandle.Value != this.GetType().TypeHandle.Value)
            {
                return false;
            }
            Token token = (Token)obj;
            return token.Column == this.Column && token.Line == this.Line;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}