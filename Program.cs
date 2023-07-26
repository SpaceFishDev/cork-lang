namespace Cork
{

    enum ValueType
    {
        ins,
        lst,
        @string,
        number
    }
    struct ObjValue
    {
        public ValueType Type;
        public object Value;

        public ObjValue(ValueType type, object value)
        {
            Type = type;
            Value = value;
        }
        public string GetStr(object val)
        {
            if (val.GetType().Name == (new List<object>()).GetType().Name)
            {
                string str = "[ ";
                foreach (object v in (List<object>)val)
                {
                    str += " " + v.ToString() + " ";
                }
                str += "]";
                return str;
            }
            if (val != null)
            {
                string? str = val.ToString();
                if (str == null)
                {
                    str = "null";
                }
                return str;
            }
            return "null";
        }

        public override string ToString()
        {
            return GetStr(Value);
        }
    }
    struct Obj
    {
        public string Name;
        public bool isFunc;
        public bool Exists = true;
        public ObjValue Value;
        public List<string> Args = new List<string>();
        public int Location;
        public Obj(string Name, bool isF, ObjValue value, int Location = 0)
        {
            this.Name = Name;
            isFunc = isF;
            Value = value;
            this.Location = Location;
        }
        public override string ToString()
        {
            string res = $"OBJ([{Name}]: {Value.ToString()})";
            return res;
        }
    }
    class Interpreter
    {
        public object? ReturnObject;
        private Visitor visitor;
        private int Position;
        private List<Obj> Objects;
        private List<int> CallStack = new List<int>();
        public Interpreter(string src)
        {
            Objects = new List<Obj>();
            visitor = new(src);
            visitor.VisitAll();
            if (visitor.parser.pTokenizer.Errors.Count != 0)
            {
                foreach (var err in visitor.parser.pTokenizer.Errors)
                {
                    Console.WriteLine(err);
                }
                Environment.Exit(-1);
            }
            foreach (Instruction ins in visitor.Instructions)
            {
                Console.WriteLine(ins);
            }
            bool hasMain = false;
            foreach (Instruction ins in visitor.Instructions)
            {
                if (ins.Opcode == InstructionType.def && ins.Args[0].ToString() == "main")
                {
                    hasMain = true;
                    break;
                }
            }
            if (!hasMain)
            {
                Console.WriteLine("ERROR: No 'main' Function Defineed");
                Environment.Exit(-1);
            }
            CollectObjects();
            foreach (Object obj in Objects)
            {
                Console.WriteLine(obj);
            }
        }
        private ValueType GetObjType(object value)
        {
            if (value.GetType().Name == (new List<object>()).GetType().Name)
            {
                return ValueType.lst;
            }
            else if (value.GetType().Name == ((double)0.1).GetType().Name)
            {
                return ValueType.number;
            }
            else if (value.GetType().Name == ((string)"").GetType().Name)
            {
                return ValueType.@string;
            }
            return ValueType.ins;
        }
        private bool ObjectExists(string? Name)
        {
            if (Name == null)
            {
                return false;
            }
            foreach (Obj obj in Objects)
            {
                if (obj.Exists && obj.Name == Name)
                {
                    return true;
                }
            }
            return false;
        }
        public string? GetStr(object obj)
        {
            string res = "";

            if (obj == null)
            {
                return "null";
            }
            if (obj != null && obj.ToString() != null && obj.ToString() == "true" || obj.ToString() == "false")
            {
                return obj.ToString();
            }
            if (obj.ToString() != null && obj.ToString().StartsWith('"') && obj.ToString().EndsWith('"'))
            {
                res += obj.ToString().Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\b", "\b").Replace("\\f", "\f");
            }
            else if (obj.GetType().Name == ((double)0).GetType().Name || obj.GetType().Name == ((string)"").GetType().Name)
            {
                res += obj.ToString();
            }
            else if (obj.GetType().Name == (new List<object>()).GetType().Name)
            {
                res += "[ ";
                foreach (object c in (List<object>)obj)
                {
                    res += " " + GetStr(c) + " ";
                }
                res += " ]";
            }
            return res;
        }
        public void Interpret()
        {
            int i = 0;
            while (!(visitor.Instructions[i].Opcode == InstructionType.def && visitor.Instructions[i].Args[0].ToString() == "main"))
            {
                ++i;
            }
            Position = i + 1;
            while (Position < visitor.Instructions.Count)
            {
                ExecuteInstruction(visitor.Instructions[Position]);
                ++Position;
            }
        }
        private void ExecuteInstruction(Instruction ins)
        {
            switch (ins.Opcode)
            {
                case InstructionType.call:
                    {
                        List<object> Arguments = new();
                        foreach (var arg in ins.Args)
                        {
                            if (arg.ToString() != ins.Args[0].ToString())
                            {
                                Arguments.Add(arg);
                            }
                        }
                        ReturnObject = ExecuteCall(ins, Arguments);
                    }
                    break;
                case InstructionType.ret:
                    {
                        if (ins.Args[0].ToString() == "main")
                        {
                            Environment.Exit(0);
                        }
                        Position = CallStack[CallStack.Count - 1];
                        CallStack.RemoveAt(CallStack.Count - 1);
                    }
                    break;
            }
            ++Position;
        }
        private object? GetObj(string Name)
        {
            foreach (Obj o in Objects)
            {
                if (o.Exists && o.Name == Name)
                {
                    if (o.isFunc)
                    {
                        List<object> a = new();
                        a.Add(o.Name);
                        return ExecuteCall(new Instruction(InstructionType.call, a), new List<object>());
                    }
                    return o.Value.Value;
                }
            }
            return null;
        }
        private string GetPrtStrSingle(object obj)
        {
            string output = "";
            output = GetStr(GetValue(obj));
            return (output == null) ? "null" : output.Replace("\"", "");
        }
        private string GetPrtStr(List<object> Arguments)
        {
            string output = "";
            foreach (object obj in Arguments)
            {

                output += GetPrtStrSingle(obj);
            }
            return output;
        }
        private object GetValue(object v)
        {
            if (v == null)
            {
                return null;
            }
            if (v.ToString() == "false")
            {
                return false;
            }
            if (v.ToString() == "true")
            {
                return true;
            }
            if (char.IsDigit(v.ToString()[0]))
            {
                return double.Parse(v.ToString());
            }
            if (v.GetType().Name == (new List<object>()).GetType().Name)
            {
                return GetValue(((List<object>)v)[0]);
            }
            if (v.GetType().Name == (new Instruction(InstructionType.call, new List<object>())).GetType().Name)
            {
                var i = (Instruction)v;
                ExecuteInstruction(i);
                return ReturnObject;
            }
            if (v.GetType().Name == ("").GetType().Name)
            {
                if (v.ToString().StartsWith("\""))
                {
                    return v.ToString();
                }
                else
                {
                    return GetValue(GetObj(v.ToString()));
                }
            }
            return v;
        }
        private object? ExecuteCall(Instruction ins, List<object> Arguments)
        {
            if (ins.Args.Count == 0)
            {
                return false;
            }
            switch (ins.Args[0].ToString())
            {
                case "to_bool":
                    {
                        if (Arguments.Count != 1)
                        {
                            return false;
                        }
                        object val = GetValue(Arguments[0]);
                        string str = GetStr(val);
                        bool isNum = int.TryParse(str, out int res);
                        if (isNum)
                        {
                            if (res > 0)
                            {
                                return true;
                            }
                            return false;
                        }
                        if (str == "true" || str == "false")
                        {
                            return bool.Parse(str);
                        }
                        return false;
                    }
                case "cmp":
                    {
                        if (Arguments.Count != 2)
                        {
                            return false;
                        }
                        object val1 = GetValue(Arguments[0]);
                        object val2 = GetValue(Arguments[1]);

                        if (val1 != null && val2 != null && val1.GetType().Name != (new List<object>()).GetType().Name && val2.GetType().Name != (new List<object>()).GetType().Name)
                        {
                            ReturnObject = GetStr(val1) == GetStr(val2);
                        }
                        return ReturnObject;
                    }
                    break;
                case "readstr":
                    {
                        ReturnObject = "\"" + Console.ReadLine() + "\"";
                        return ReturnObject;
                    }
                case "return":
                    {
                        if (visitor.Instructions[Position + 1].Opcode == InstructionType.ret)
                        {
                            ReturnObject = GetValue(Arguments[0]);
                        }
                        return GetValue(Arguments[0]);
                    }
                case "print":
                    {
                        string output = GetPrtStr(Arguments);
                        Console.Write(output.Replace("null", ""));
                        return (double)0;
                    }
                case "println":
                    {
                        string output = GetPrtStr(Arguments);
                        Console.WriteLine(output.Replace("null", ""));
                        return (double)0;
                    }
                case "exit":
                    {
                        Environment.Exit(0);
                    }
                    break;
                case "if":
                    {
                        if (Arguments.Count != 2)
                        {
                            Console.WriteLine("Function 'if' takes in 2 arguments.");
                            Environment.Exit(-1);
                        }
                        string ArgA = Arguments[0].ToString();
                        string ArgB = Arguments[1].ToString();
                        if (!ObjectExists(ArgB))
                        {
                            Console.WriteLine($"ERROR: Function '{ArgB}' doesnt exist.");
                            Environment.Exit(-1);
                        }
                        if (ArgA.ToString() == "true")
                        {
                            Instruction inst = new Instruction(InstructionType.call, new List<object>());
                            inst.Args.Add(ArgB.ToString());
                            ExecuteCall(inst, new List<object>());
                        }
                        else if (ArgA.ToString() != "false")
                        {
                            if (ObjectExists(ArgA))
                            {
                                foreach (Obj obj in Objects)
                                {
                                    if (obj.Exists && obj.Name == ArgA && obj.isFunc)
                                    {
                                        break;
                                    }
                                    else if (!obj.isFunc && obj.Exists && obj.Name == ArgA)
                                    {
                                        Console.WriteLine("ERROR; if uses a function as first arg.");
                                        Environment.Exit(-1);
                                    }
                                }
                            }
                            Instruction inst = new Instruction(InstructionType.call, new List<object>());
                            inst.Args.Add(ArgA.ToString());
                            var x = ExecuteCall(inst, new List<object>());
                            if (x.ToString() == "1" || x.ToString() == "true")
                            {
                                Instruction inst2 = new Instruction(InstructionType.call, new List<object>());
                                inst2.Args.Add(ArgB.ToString());
                                ExecuteCall(inst2, new List<object>());
                            }
                        }
                    }
                    break;
            }
            if (!ObjectExists(ins.Args[0].ToString()))
            {
                visitor.parser.pTokenizer.Errors.Add(new ErrInfo(1, $"Object {ins.Args[0].ToString()} doesnt exist!", -1, 0));
                return null;
            }
            foreach (Obj obj in Objects)
            {
                if (obj.Name == ins.Args[0].ToString())
                {
                    if (!obj.isFunc)
                    {
                        visitor.parser.pTokenizer.Errors.Add(new ErrInfo(1, $"Object {ins.Args[0].ToString()} is not a function.", -1, -1));
                        return null;
                    }
                    CallStack.Add(Position + 1);
                    int i = 0;
                    foreach (string a in obj.Args)
                    {
                        var val = GetValue(Arguments[i]);
                        Objects.Add(new(a, false, new(GetObjType(val), val)));
                        ++i;
                    }
                    Position = obj.Location - 1;
                    while (visitor.Instructions[Position].Opcode != InstructionType.ret)
                    {
                        ExecuteInstruction(visitor.Instructions[Position]);
                    }
                    Position = CallStack[0];
                    CallStack.RemoveAt(0);
                    return ReturnObject;
                    break;
                }
            }
            return 1;
        }
        private Obj? ParseInstruction(Instruction ins, ref int idx)
        {
            switch (ins.Opcode)
            {
                case InstructionType.def:
                    {

                        if (ObjectExists(ins.Args[0].ToString()))
                        {
                            visitor.parser.pTokenizer.Errors.Add(new(1, $"Object {ins.Args[0]} redefined.", -1, -1));
                        }
                        List<object> instructions = new List<object>();
                        ++idx;
                        int startIdx = idx;
                        while (visitor.Instructions[idx].Opcode != InstructionType.ret)
                        {
                            instructions.Add(visitor.Instructions[idx]);
                            ++idx;
                        }
                        ++idx;
                        Obj obj = new(ins.Args[0].ToString(), true, new ObjValue(ValueType.ins, instructions), startIdx);
                        foreach (var arg in ins.Args)
                        {
                            if (arg.ToString() != ins.Args[0].ToString())
                            {
                                obj.Args.Add(arg.ToString());
                            }
                        }
                        return obj;
                    }
                case InstructionType.variable:
                    {
                        Obj obj = new(ins.Args[0].ToString(), false, new(GetObjType(ins.Args[1]), ins.Args[1]));
                        ++idx;
                        return obj;
                    }
            }
            ++idx;
            return null;
        }
        private void CollectObjects()
        {
            for (int idx = 0; idx < visitor.Instructions.Count;)
            {
                int startIdx = idx;
                Obj? obj = ParseInstruction(visitor.Instructions[idx], ref idx);
                if (obj != null)
                {
                    Objects.Add((Obj)obj);
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] Args)
        {
            string mainFile = "main.cork";
            if (Args.Length != 0)
            {
                mainFile = Args[0];
            }
            string inputFile = File.ReadAllText(mainFile);
            Interpreter interpreter = new(inputFile);
            Console.WriteLine("\nOutput:\n");
            interpreter.Interpret();
        }
    }
}