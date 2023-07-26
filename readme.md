# CORK
Cork is an interpreted functional language.<br>
Cork doesnt follow the normal functional paradigm. Yes it is functional but variables are mutable and all variables (besides function arguments) are global.
- The interpreter was written in C# This is because I didnt want to have to deal with generics in C since this is a toy project.
- The parser is just recursive decent so its not very advanced but its quite dynamic working pretty well.
- The enterpreter is quite dynamic so it does quite alot.

# Hello World!
```py
main = ("Hello World!")println;
```
Yes, that's right the function calls are backwards. Why did I decide to use this?
Well in functional languages alot of things get complicated because you'd have to consider the logic backwards. Whilstt in this language whats on the left is done first not the other way round.

# How to build.
The project is built for Dotnet 7.0 but the features used are all supported by 6.0 so either will work.
If you use Dotnet 6.0 you will need to change the csproj file.
