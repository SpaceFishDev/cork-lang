# NOTES
## Basic outline:
- Parser is recursive decent.
- Order of operations is left to right
- Compiler is also an interpreter
- Targets planned: linux and interpreted
- Bytecode is first compiled.
## Grammar
```
program: function*;
function: identifier '=' (function_call'|')* function_call';' || const ';' // all functions need 1 function_call or value 
function_call: arg* '$' identifier
arg:
	string
	| identifier
	| number
	| list_init
list_init:
	'[' arg* ']''{''}' 
	| '['']''{'number'}'
```
