namespace Cork
{
    class Node
    {
        public NodeType Type;
        public Token nodeToken;
        public Node? Parent;
        public List<Node> Children;

        public Node(Token token, Node? parent, NodeType type)
        {
            nodeToken = token;
            Parent = parent;
            Type = type;
            Children = new();
        }
        public string GetStr(Node n)
        {
            return $"[{n.Type}] : {n.nodeToken.ToString()}";
        }
        public string indented(int indent)
        {
            string res = "";
            for (int i = 0; i < indent; ++i)
            {
                res += " ";
            }
            return res;
        }

        public override string ToString()
        {
            return $"[{Type}]: {nodeToken.ToString()}";
        }
    }
}