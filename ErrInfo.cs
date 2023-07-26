namespace Cork
{
    struct ErrInfo
    {
        public int Severity;
        public int Line;
        public int Column;
        public string Error;

        public ErrInfo(int severity, string err, int l, int c)
        {
            Severity = severity;
            Error = err;
            Line = l;
            Column = c;
        }

        public override string ToString()
        {
            if (Line == -1)
            {
                return $"ERROR: {Error}";
            }
            return $"ERROR[{Line},{Column}]: {Error}";
        }
    }
}