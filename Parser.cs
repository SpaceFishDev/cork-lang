namespace Cork
{
    class Parser
    {
        public Tokenizer pTokenizer;
        private int Position;
        public Node Root;
        public Node Parent;
        public Token Current { get { return pTokenizer.Tokens[Position]; } }
        public void PrintNode(Node n, int indent)
        {
            string indt = "";
            for (int i = 0; i < indent; ++i)
            {
                indt += "- ";
            }
            Console.WriteLine(indt + n.ToString());
            indent++;
            if (n.Children.Count != 0)
            {
                Console.WriteLine(indt + "{");
                foreach (var child in n.Children)
                {
                    PrintNode(child, indent);
                }
                Console.WriteLine(indt + "}");
            }
        }
        public Parser(string Src)
        {
            pTokenizer = new(Src);
            Position = 0;
            Root = new Node(new Token(0, 0, "PROGRAM", TokenType.prog), null, NodeType.program);
            Parent = Root;
            pTokenizer.LexAll();
            if (pTokenizer.Errors.Count != 0)
            {
                foreach (var err in pTokenizer.Errors)
                {
                    Console.WriteLine(err);
                }
                Environment.Exit(-1);
            }
            foreach (var token in pTokenizer.Tokens)
            {
                Console.WriteLine(token);
            }
        }
        private bool Is(TokenType type, bool Err = false)
        {
            if (Position < pTokenizer.Tokens.Count && pTokenizer.Tokens[Position].Type != type)
            {
                if (Err)
                {
                    pTokenizer.Errors.Add
                    (
                        new ErrInfo
                        (
                            1,
                            $"Unexpected Token of Type {pTokenizer.Tokens[Position].Type} Expected: {type}",
                            pTokenizer.Tokens[Position].Line,
                            pTokenizer.Tokens[Position].Column
                        )
                    );
                }
                return false;
            }
            return true;
        }
        private bool IsContent(string Content, bool Err = false)
        {
            if (pTokenizer.Tokens[Position].Text != Content)
            {
                if (Err)
                {
                    pTokenizer.Errors.Add
                    (
                        new ErrInfo
                        (
                            1,
                            $"Unexpected Token of Content {pTokenizer.Tokens[Position].Text} Expected: {Content}",
                            pTokenizer.Tokens[Position].Line,
                            pTokenizer.Tokens[Position].Column
                        )
                    );
                }
                return false;
            }
            return true;
        }
        private bool Expect(TokenType type, bool Err = false)
        {
            if (Is(TokenType.eof))
            {
                if (Err)
                {
                    pTokenizer.Errors.Add
                    (
                        new ErrInfo
                        (
                            1,
                            $"Unexpected Token of Type {pTokenizer.Tokens[Position].Type} Expected: {type}",
                            pTokenizer.Tokens[Position].Line,
                            pTokenizer.Tokens[Position].Column
                        )
                    );
                }
                return false;
            }
            if (pTokenizer.Tokens[Position + 1].Type != type)
            {
                if (Err)
                {
                    pTokenizer.Errors.Add
                    (
                        new ErrInfo
                        (
                            1,
                            $"Unexpected Token of Type {pTokenizer.Tokens[Position + 1].Type} Expected: {type}",
                            pTokenizer.Tokens[Position + 1].Line,
                            pTokenizer.Tokens[Position + 1].Column
                        )
                    );
                }
                return false;
            }
            return true;
        }
        private void Next()
        {
            ++Position;
        }
        private bool ExpectContent(string Content, bool Err = false)
        {
            if (pTokenizer.Tokens[Position + 1].Text != Content)
            {
                if (Err)
                {
                    pTokenizer.Errors.Add
                    (
                        new ErrInfo
                        (
                            1,
                            $"Unexpected Token of Content '{pTokenizer.Tokens[Position + 1].Text}' Expected: '{Content}'",
                            pTokenizer.Tokens[Position + 1].Line,
                            pTokenizer.Tokens[Position + 1].Column
                        )
                    );
                }
                return false;
            }
            return true;
        }
        private void Reverse()
        {
            --Position;
        }
        private Node ParseConstOrId(Node parent)
        {
            if (Current.Type == TokenType.open_list)
            {
                return ParseBasic();
            }
            if (Current.Type == TokenType.open)
            {
                int scope = 1;
                while (true)
                {
                    Next();
                    if (Is(TokenType.close))
                    {
                        --scope;
                        if (scope == 0)
                        {
                            break;
                        }
                    }
                    if (Is(TokenType.open))
                    {
                        ++scope;
                    }
                }
                Next();
                Node n = ParseBasic();
                if (n != null)
                {
                    n.Parent = parent;
                }
                return n;
            }
            Node c = new(Current, parent, NodeType.constant);
            return c;
        }

        private Node? ParseBasic()
        {
            switch (Current.Type)
            {
                case TokenType.open_list:
                    {
                        Node ls = new(Current, Parent, NodeType.list);
                        ls.nodeToken.Text = "[]";
                        if (Expect(TokenType.close_list))
                        {
                            Next();
                            Expect(TokenType.open_size, true);
                            Next();
                            Expect(TokenType.number, true);
                            Next();
                            ls.Children.Add(new(Current, ls, NodeType.list_size));
                            Next();
                        }
                        else
                        {
                            Next();
                            while (!Is(TokenType.close_list))
                            {
                                Node n = ParseConstOrId(ls);
                                if (n != null)
                                {
                                    ls.Children.Add(n);
                                }
                                Next();
                            }
                            Expect(TokenType.open_size, true);
                            Next();
                            Expect(TokenType.close_size, true);
                            Next();
                        }
                        return ls;
                    }
                case TokenType.number:
                case TokenType.@string:
                    {
                        Node n = new(Current, Parent, NodeType.constant);
                        return n;
                    }
                case TokenType.open:
                    {
                        int scope = 1;
                        while (true)
                        {
                            Next();
                            if (Is(TokenType.open))
                            {
                                ++scope;
                            }
                            if (Is(TokenType.close))
                            {
                                if (scope == 1)
                                {
                                    break;
                                }
                                --scope;
                            }
                        }
                        Next();
                        return ParseBasic();
                    }
                case TokenType.identifier:
                    {
                        Token token = Current;
                        Node func = new(Current, Parent, NodeType.call);
                        int scope = 1;
                        bool contains_func = false;
                        Reverse();
                        if (IsContent("="))
                        {
                            Next();
                            Node n = new(Current, Parent, NodeType.constant);
                            Expect(TokenType.end_of_expr, true);
                            Next();
                            return n;
                        }
                        IsContent(")", true);
                        while (true)
                        {
                            Reverse();
                            if (Is(TokenType.open))
                            {
                                --scope;
                                if (scope == 0)
                                {
                                    break;
                                }
                            }
                            if (Is(TokenType.close))
                            {
                                contains_func = true;
                                ++scope;
                            }
                        }
                        Next();
                        scope = 1;
                        bool hasfArgs = false;
                        while (!Is(TokenType.close) && !Is(TokenType.end_of_expr))
                        {
                            if (Is(TokenType.open))
                            {
                                if (!Expect(TokenType.close))
                                {
                                    hasfArgs = true;
                                }
                                Node n = ParseBasic();
                                func.Children.Add(n);
                                Next();
                                Next();
                                contains_func = true;
                            }
                            else
                            {
                                func.Children.Add(ParseConstOrId(func));
                                Next();
                            }
                        }
                        if (contains_func && hasfArgs)
                        {
                            func.Children.RemoveAt(func.Children.Count - 1);
                        }
                        Next();
                        return func;
                    }
            }
            return null;
        }
        private Node ParsePrimary()
        {
            switch (Current.Type)
            {
                case TokenType.end_of_expr:
                    {
                        Next();
                        return ParsePrimary();
                    }
                case TokenType.identifier:
                    {
                        Node left = new(Current, Parent, NodeType.function);
                        if (!ExpectContent("="))
                        {
                            Next();
                            while (Is(TokenType.identifier))
                            {
                                Node arg = new(Current, left, NodeType.arg);
                                left.Children.Add(arg);
                                Next();
                            }
                        }
                        else
                        {
                            Next();
                        }
                        Reverse();
                        if (ExpectContent(";"))
                        {
                            Next();
                            Next();
                            return Root;
                        }
                        ExpectContent("=", true);
                        Next();
                        Next();
                        while (!Is(TokenType.end_of_expr))
                        {
                            Node? right = ParseBasic();
                            if (right != null)
                            {
                                right.Parent = left;
                                left.Children.Add(right);
                                if (!Expect(TokenType.comma))
                                {
                                    Reverse();
                                    if (!Is(TokenType.end_of_expr))
                                    {
                                        Next();
                                        Expect(TokenType.end_of_expr, true);
                                    }
                                    else
                                    {
                                        Next();
                                    }
                                    Next();
                                    break;
                                }
                                else
                                {
                                    Expect(TokenType.comma, true);
                                    Next();
                                }
                            }
                            if (Is(TokenType.comma))
                            {
                                Next();
                            }
                        }
                        Parent.Children.Add(left);
                        return left;
                    }
            }
            return Root;
        }
        public void Parse()
        {
            bool end = false;
            while (!end && Position < pTokenizer.Tokens.Count && !Is(TokenType.eof))
            {
                ParsePrimary();
            }
            PrintNode(Root, 0);
            if (pTokenizer.Errors.Count != 0)
            {
                foreach (var err in pTokenizer.Errors)
                {
                    Console.WriteLine(err);
                }
                Environment.Exit(-1);
            }
        }
    }
}