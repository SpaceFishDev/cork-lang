namespace Cork
{
    enum TokenType
    {
        identifier,
        number,
        symbol,
        end_of_expr,
        @string,
        eof,
        err,
        prog,
        open,
        close,
        open_list,
        close_list,
        open_size,
        close_size,
        comma,
    }
}