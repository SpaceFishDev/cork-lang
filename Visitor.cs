namespace Cork
{
    class Visitor
    {
        public List<Instruction> Instructions;
        private Node Root;
        public Parser parser;

        public Visitor(string src)
        {
            parser = new(src);
            parser.Parse();
            Root = parser.Root;
            Instructions = new();
        }
        public void VisitAll()
        {
            RestructureParents(Root, Root);
            Visit(Root);
        }
        private void RestructureParents(Node Parent, Node Current)
        {
            if (Current.Type != NodeType.program)
            {
                Current.Parent = Parent;
            }
            foreach (var child in Current.Children)
            {
                RestructureParents(Current, child);
            }
        }
        private List<object> listNodeToObjects(Node ls)
        {
            List<object> res = new List<object>();
            if (ls.Children.Count == 1 && ls.Children[0].Type == NodeType.list_size)
            {
                bool isInt = int.TryParse(ls.Children[0].nodeToken.Text, out int size);
                if (!isInt)
                {
                    parser.pTokenizer.Errors.Add(new(1, "Size of a list cannot have a decimal.", ls.nodeToken.Line, ls.nodeToken.Column));
                }
                else
                {
                    for (int i = 0; i < size; ++i)
                    {
                        res.Add(0);
                    }
                }
            }
            else
            {
                foreach (Node c in ls.Children)
                {
                    if (c.Type == NodeType.list)
                    {
                        List<object> r = listNodeToObjects(c);
                        foreach (object n in r)
                        {
                            bool isNum = double.TryParse(n.ToString(), out double n_d);
                            if (isNum)
                            {
                                res.Add(n_d);
                            }
                            else
                            {
                                if (n.ToString() == "true")
                                {
                                    res.Add(true);
                                }
                                else if (n.ToString() == "false")
                                {
                                    res.Add(false);
                                }
                                else
                                {
                                    res.Add(n);
                                }
                            }
                        }
                    }
                    else
                    {
                        res.Add(c.nodeToken.Text);
                    }
                }
            }
            return res;
        }
        private void Visit(Node current)
        {
            switch (current.Type)
            {
                case NodeType.function:
                    {
                        var ins = new Instruction(InstructionType.def, new List<object>());
                        ins.Args.Add(current.nodeToken.Text);
                        Instructions.Add(ins);
                    }
                    break;
                case NodeType.arg:
                    {
                        Instructions[Instructions.Count - 1].Args.Add(current.nodeToken.Text);
                    }
                    break;
                case NodeType.call:
                    {
                        var ins = new Instruction(InstructionType.call, new List<object>());
                        ins.Args.Add(current.nodeToken.Text);
                        if (current.Parent.Type == NodeType.call)
                        {
                            Instructions[Instructions.Count - 1].Args.Add(ins);
                        }
                        else
                        {
                            Instructions.Add(ins);
                        }
                    }
                    break;
                case NodeType.list:
                    {
                        if (current.Parent.Type == NodeType.call)
                        {
                            List<object> ls = listNodeToObjects(current);
                            Instructions[Instructions.Count - 1].Args.Add(ls);
                        }
                        else if (current.Parent.Type == NodeType.function)
                        {
                            List<object> ls = listNodeToObjects(current);
                            Instruction ins = new(InstructionType.variable, new List<object>());
                            ins.Args.Add(Instructions[Instructions.Count - 1].Args[0]);
                            ins.Args.Add(ls);
                            Instructions[Instructions.Count - 1] = ins;
                        }
                    }
                    break;
                case NodeType.constant:
                    {
                        if (current.Parent.Type != NodeType.list)
                        {
                            if (current.Parent.Type == NodeType.call)
                            {
                                if (current.nodeToken.Type != TokenType.comma)
                                {

                                    if (current.Parent.Parent.Type == NodeType.call)
                                    {
                                        int idx = 0;
                                        while (idx < Instructions[Instructions.Count - 1].Args.Count)
                                        {
                                            if (Instructions[Instructions.Count - 1].Args[idx].GetType().Name == (new Instruction(InstructionType.call, new List<object>())).GetType().Name)
                                            {
                                                if (((Instruction)Instructions[Instructions.Count - 1].Args[idx]).Args[0] == current.Parent.nodeToken.Text)
                                                {
                                                    break;
                                                }
                                            }
                                            ++idx;
                                        }
                                        switch (current.nodeToken.Type)
                                        {
                                            case TokenType.identifier:
                                            case TokenType.number:
                                                {
                                                    bool isNum = double.TryParse(current.nodeToken.Text, out double n);
                                                    if (isNum)
                                                    {
                                                        ((Instruction)Instructions[Instructions.Count - 1].Args[idx]).Args.Add(n);
                                                    }
                                                    else
                                                    {
                                                        ((Instruction)Instructions[Instructions.Count - 1].Args[idx]).Args.Add(current.nodeToken.Text);
                                                    }
                                                }
                                                break;
                                            case TokenType.@string:
                                                {
                                                    ((Instruction)Instructions[Instructions.Count - 1].Args[idx]).Args.Add("\"" + current.nodeToken.Text + "\"");
                                                }
                                                break;
                                        }

                                    }
                                    else
                                    {
                                        switch (current.nodeToken.Type)
                                        {
                                            case TokenType.identifier:
                                            case TokenType.number:
                                                {
                                                    bool isNum = double.TryParse(current.nodeToken.Text, out double n);
                                                    if (isNum)
                                                    {
                                                        Instructions[Instructions.Count - 1].Args.Add(n);
                                                    }
                                                    else
                                                    {
                                                        Instructions[Instructions.Count - 1].Args.Add(current.nodeToken.Text);
                                                    }
                                                }
                                                break;
                                            case TokenType.@string:
                                                {
                                                    Instructions[Instructions.Count - 1].Args.Add("\"" + current.nodeToken.Text + "\"");
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var ins = new Instruction(InstructionType.variable, new List<object>());
                                if (Instructions[Instructions.Count - 1].Args.Count != 1)
                                {
                                    parser.pTokenizer.Errors.Add(new ErrInfo(1, "Variables cannot take in arguments.\n", current.nodeToken.Line, current.nodeToken.Line));
                                }
                                ins.Args.Add(Instructions[Instructions.Count - 1].Args[0]);
                                Instructions[Instructions.Count - 1] = ins;
                                switch (current.nodeToken.Type)
                                {
                                    case TokenType.identifier:
                                    case TokenType.number:
                                        {
                                            Instructions[Instructions.Count - 1].Args.Add(current.nodeToken.Text);
                                        }
                                        break;
                                    case TokenType.@string:
                                        {
                                            Instructions[Instructions.Count - 1].Args.Add("\"" + current.nodeToken.Text + "\"");
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    break;
            }
            foreach (Node child in current.Children)
            {
                Visit(child);
            }
            switch (current.Type)
            {
                case NodeType.function:
                    {
                        Instructions.Add(new(InstructionType.ret, new List<object>()));
                        Instructions[Instructions.Count - 1].Args.Add(current.nodeToken.Text);
                    }
                    break;
            }
        }
    }
}