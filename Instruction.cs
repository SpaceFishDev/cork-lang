namespace Cork
{
    struct Instruction
    {
        public InstructionType Opcode;
        public List<object> Args;
        public Instruction(InstructionType opcode, List<object> arguments)
        {
            Opcode = opcode;
            Args = arguments;
        }
        private string lsStr(List<object> ls)
        {
            string res = "[ ";
            foreach (object a in ls)
            {
                if (a.GetType().Name == (new List<object>()).GetType().Name)
                {
                    res += " " + lsStr((List<object>)a) + " ";
                }
                res += " " + a.ToString() + " ";
            }
            res += " ]";
            return res;
        }
        public override string ToString()
        {
            string str = $"[{Opcode}](";
            foreach (object arg in Args)
            {
                if (arg.GetType().Name == (new List<object>()).GetType().Name)
                {
                    List<object> ls = (List<object>)arg;
                    str += $":{lsStr(ls)}:";
                }
                else
                {
                    str += $":{arg}:";
                }
            }
            str += ")";
            return str;
        }
    }
}