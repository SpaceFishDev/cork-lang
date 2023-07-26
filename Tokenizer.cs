namespace Cork
{
    class Tokenizer
    {
        public List<ErrInfo> Errors;
        private int Position;
        private int Line;
        private int Column;
        private string Src;

        public List<Token> Tokens;
        private char Current { get { return (Position < Src.Length) ? Src[Position] : '\0'; } }

        public Tokenizer(string src)
        {
            Src = src;
            Tokens = new();
            Errors = new();
            Line = 1;
            Column = 0;
            Position = 0;
        }
        private void Next()
        {
            ++Position;
            ++Column;
        }
        private Token Lex()
        {
            if (Position >= Src.Length)
            {
                return new Token(Line, Column, "eof", TokenType.eof);
            }
            switch (Current)
            {
                case '[':
                    {
                        Next();
                        return new Token(Line, Column, "[", TokenType.open_list);
                    }
                case ']':
                    {
                        Next();
                        return new Token(Line, Column, "]", TokenType.close_list);
                    }
                case '{':
                    {
                        Next();
                        return new Token(Line, Column, "{", TokenType.open_size);
                    }
                case '}':
                    {
                        Next();
                        return new Token(Line, Column, "}", TokenType.close_size);
                    }
                case '/':
                    {
                        Next();
                        if (Current == '/')
                        {
                            Next();
                            while (Current != '\0' && Current != '\n')
                            {
                                Next();
                            }
                        }
                        else
                        {
                            Errors.Add(new ErrInfo(1, $"Unexpected Character '/'\n", Line, Column));
                        }
                        return Lex();
                    }
                case '(':
                    {
                        Next();
                        return new Token(Line, Column - 1, "(", TokenType.open);
                    }
                case ')':
                    {
                        Next();
                        return new Token(Line, Column - 1, ")", TokenType.close);
                    }
                case '\'':
                case '"':
                    {
                        int StartColumn = Column;
                        char start = Current;
                        Next();
                        string Result = "";
                        while (Current != '\0' && Current != start)
                        {
                            Result += Current;
                            Next();
                        }
                        Next();
                        return new Token(Line, StartColumn, "\"" + Result + "\"", TokenType.@string);
                    }
                case ',':
                    {
                        Next();
                        return new(Line, Column - 1, Current.ToString(), TokenType.comma);
                    }
                case ';':
                    {
                        Token token = new Token(Line, Column, Current.ToString(), TokenType.end_of_expr);
                        Next();
                        return token;
                    }
                case '=':
                    {
                        Token token = new Token(Line, Column, Current.ToString(), TokenType.symbol);
                        Next();
                        return token;
                    }
                case '\n':
                    {
                        ++Line;
                        ++Position;
                        Column = 0;
                        return Lex();
                    }
                case ' ':
                case '\t':
                    {
                        Next();
                        return Lex();
                    }

            }
            if (char.IsLetter(Current) || Current == '_')
            {
                string Result = "";
                int StartColumn = Column;
                while (char.IsLetterOrDigit(Current) || Current == '_')
                {
                    Result += Current;
                    Next();
                }
                return new Token(Line, StartColumn, Result, TokenType.identifier);
            }
            if (char.IsDigit(Current))
            {
                string Result = "";
                int StartColumn = Column;
                int numDecimal = 0;
                while (char.IsDigit(Current) || (Current == '.' && numDecimal == 0))
                {
                    if (Current == '.')
                    {
                        ++numDecimal;
                    }
                    Result += Current;
                    Next();
                }
                return new Token(Line, StartColumn, Result, TokenType.number);
            }

            Errors.Add(new ErrInfo(1, $"Unexpected Character '{Current}'", Line, Column));
            Next();
            return new Token(Line, Column, "err", TokenType.err);
        }

        public void LexAll()
        {
            while (true)
            {
                Token token = Lex();
                Tokens.Add(token);
                if (token.Type == TokenType.eof)
                {
                    return;
                }
            }
        }

    }
}