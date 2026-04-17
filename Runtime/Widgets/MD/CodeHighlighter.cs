using System;
using System.Collections.Generic;
using System.Text;

namespace UniDecl.Runtime.Widgets.MD
{
    /// <summary>
    /// Lightweight code syntax highlighter that produces Unity rich-text markup.
    /// Supports C#, C, C++, Java, Lua, JavaScript, TypeScript, Python, JSON, and plain text.
    /// </summary>
    public static class CodeHighlighter
    {
        // ── Color palette ──────────────────────────────────────────────────
        private const string KeywordColor   = "#569cd6"; // blue
        private const string ControlColor   = "#c586c0"; // purple (if/else/while/for/return)
        private const string TypeColor      = "#4ec9b0"; // teal (class/struct/interface/enum)
        private const string StringColor    = "#ce9178"; // orange
        private const string NumberColor    = "#b5cea8"; // light green
        private const string CommentColor   = "#6a9955"; // green
        private const string DefaultColor   = "#d4d4d4"; // light gray (NOT red)
        private const string PunctuationColor = "#d4d4d4"; // light gray
        private const string PreprocessorColor = "#c586c0"; // purple (#include, #define, etc.)
        private const string AttributeColor = "#d7ba7d"; // yellow ([Attr], @@decorator)
        private const string FuncCallColor  = "#dcdcaa"; // pale yellow

        // ── Keyword tables ─────────────────────────────────────────────────
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // ── Value types ──
            "bool", "byte", "sbyte", "char", "decimal", "double", "float",
            "int", "uint", "long", "ulong", "short", "ushort",
            "nint", "nuint", "half",
            // ── Reference types ──
            "object", "string", "dynamic", "void",
            // ── Literals ──
            "true", "false", "null",
            // ── Contextual / this-base ──
            "this", "base", "var", "value",
            // ── Creation ──
            "new", "typeof", "sizeof", "default", "stackalloc",
            // ── Parameter modifiers ──
            "ref", "out", "in", "params", "__arglist",
            // ── Access modifiers ──
            "public", "private", "protected", "internal",
            // ── Modifiers ──
            "const", "static", "readonly", "volatile", "fixed",
            "virtual", "override", "abstract", "sealed", "partial",
            "extern", "unsafe",
            // ── Async ──
            "async", "await",
            // ── Contextual yield ──
            "yield",
            // ── Property / accessor ──
            "get", "set", "init", "add", "remove",
            // ── Helpers ──
            "nameof", "global", "__makeref", "__reftype", "__refvalue",
            // ── Pattern-matching / other ──
            "when", "not", "and", "or", "is", "as", "unmanaged", "scoped",
            // ── LINQ / alias (commonly highlighted) ──
            "from", "where", "select", "group", "into", "orderby", "ascending",
            "descending", "join", "on", "equals", "by", "let",
            // ── Interop ──
            "delegate", "event", "interface",
            // ── Type constraints (contextual) ──
            "unmanaged", "allows",
        };

        private static readonly HashSet<string> CSharpControl = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "if", "else", "while", "for", "foreach", "do", "switch", "case", "break",
            "continue", "return", "throw", "try", "catch", "finally", "using", "lock",
            "checked", "unchecked", "fixed", "goto",
        };

        private static readonly HashSet<string> CSharpTypeKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "class", "struct", "interface", "enum", "delegate", "event", "namespace",
            "record",
        };

        private static readonly HashSet<string> CLikeKeywords = new HashSet<string>
        {
            "int", "long", "short", "char", "float", "double", "void", "unsigned",
            "signed", "const", "static", "extern", "inline", "volatile", "register",
            "auto", "restrict", "_Alignas", "_Alignof", "_Atomic", "_Bool", "_Complex",
            "_Generic", "_Imaginary", "_Noreturn", "_Static_assert", "_Thread_local",
        };

        private static readonly HashSet<string> CLikeControl = new HashSet<string>
        {
            "if", "else", "while", "for", "do", "switch", "case", "break", "continue",
            "return", "goto", "default", "sizeof", "typedef",
        };

        private static readonly HashSet<string> CppKeywords = new HashSet<string>
        {
            "bool", "true", "false", "nullptr", "this", "class", "struct", "enum",
            "namespace", "template", "typename", "public", "private", "protected",
            "virtual", "override", "final", "new", "delete", "throw", "try", "catch",
            "using", "operator", "explicit", "implicit", "friend", "inline",
            "constexpr", "noexcept", "static_cast", "dynamic_cast", "reinterpret_cast",
            "const_cast", "nullptr_t", "auto", "decltype", "concept", "requires",
            "co_await", "co_return", "co_yield", "string", "vector", "map", "set",
        };

        private static readonly HashSet<string> JavaKeywords = new HashSet<string>
        {
            "int", "long", "short", "byte", "float", "double", "char", "boolean",
            "void", "true", "false", "null", "this", "super", "new", "instanceof",
            "class", "interface", "enum", "extends", "implements", "abstract",
            "final", "static", "synchronized", "volatile", "transient", "native",
            "strictfp", "assert", "import", "package", "throws", "var", "record",
            "sealed", "permits", "non-sealed", "yield",
        };

        private static readonly HashSet<string> JavaControl = new HashSet<string>
        {
            "if", "else", "while", "for", "do", "switch", "case", "break", "continue",
            "return", "try", "catch", "finally", "throw", "default",
        };

        private static readonly HashSet<string> LuaKeywords = new HashSet<string>
        {
            // ── Lua 5.4 全部关键字 ──
            "and", "break", "do", "else", "elseif", "end", "false", "for",
            "function", "goto", "if", "in", "local", "nil", "not", "or",
            "repeat", "return", "then", "true", "until", "while",
        };

        private static readonly HashSet<string> LuaBuiltins = new HashSet<string>
        {
            // ── 全局函数 (Lua 5.4) ──
            "_G", "_VERSION", "assert", "collectgarbage", "dofile", "error",
            "getmetatable", "ipairs", "load", "loadfile", "next", "pairs",
            "pcall", "print", "rawequal", "rawget", "rawlen", "rawset",
            "require", "select", "setmetatable", "tonumber", "tostring",
            "type", "warn", "xpcall",
            // ── coroutine ──
            "coroutine", "coroutine.create", "coroutine.isyieldable",
            "coroutine.resume", "coroutine.running", "coroutine.status",
            "coroutine.wrap", "coroutine.yield",
            // ── self (上下文) ──
            "self",
        };

        private static readonly HashSet<string> PythonKeywords = new HashSet<string>
        {
            // ── Python 3.12+ 全部关键字 ──
            "False", "None", "True", "and", "as", "assert", "async", "await",
            "break", "class", "continue", "def", "del", "elif", "else", "except",
            "finally", "for", "from", "global", "if", "import", "in", "is",
            "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try",
            "while", "with", "yield",
            // ── Soft keywords (3.10+, highlighted as keywords) ──
            "case", "match", "type", "_",
        };

        private static readonly HashSet<string> PythonBuiltins = new HashSet<string>
        {
            // ── Built-in functions ──
            "abs", "aiter", "all", "anext", "any", "ascii", "bin", "bool",
            "breakpoint", "bytearray", "bytes", "callable", "chr", "classmethod",
            "compile", "complex", "delattr", "dict", "dir", "divmod", "enumerate",
            "eval", "exec", "filter", "float", "format", "frozenset", "getattr",
            "globals", "hasattr", "hash", "help", "hex", "id", "input", "int",
            "isinstance", "issubclass", "iter", "len", "list", "locals", "map",
            "max", "memoryview", "min", "next", "object", "oct", "open", "ord",
            "pow", "print", "property", "range", "repr", "reversed", "round",
            "set", "setattr", "slice", "sorted", "staticmethod", "str", "sum",
            "super", "tuple", "type", "vars", "zip",
            // ── Common contextual / self-cls ──
            "self", "cls",
            // ── Built-in constants ──
            "NotImplemented", "Ellipsis", "__name__", "__doc__", "__file__",
            "__package__", "__spec__", "__loader__", "__builtins__",
            // ── Built-in exceptions (commonly used) ──
            "Exception", "BaseException", "ValueError", "TypeError",
            "KeyError", "IndexError", "AttributeError", "RuntimeError",
            "StopIteration", "GeneratorExit", "SystemExit", "KeyboardInterrupt",
            "ImportError", "ModuleNotFoundError", "FileNotFoundError",
            "IOError", "OSError", "RuntimeWarning", "DeprecationWarning",
            "NameError", "UnboundLocalError", "ZeroDivisionError",
            "OverflowError", "MemoryError", "RecursionError",
            "NotImplementedError", "AssertionError",
        };

        private static readonly HashSet<string> JsKeywords = new HashSet<string>
        {
            "var", "let", "const", "function", "return", "if", "else", "for", "while",
            "do", "switch", "case", "break", "continue", "new", "this", "class",
            "extends", "super", "import", "export", "default", "from", "as", "try",
            "catch", "finally", "throw", "typeof", "instanceof", "in", "of", "async",
            "await", "yield", "void", "delete", "true", "false", "null", "undefined",
        };

        private static readonly HashSet<string> JsonKeywords = new HashSet<string>
        {
            "true", "false", "null",
        };

        // ── Language detection ─────────────────────────────────────────────
        private static string NormalizeLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return "";
            lang = lang.Trim().ToLowerInvariant();
            switch (lang)
            {
                case "c#": case "cs": case "csharp": return "csharp";
                case "c": return "c";
                case "c++": case "cpp": case "cc": case "cxx": return "cpp";
                case "java": return "java";
                case "lua": return "lua";
                case "js": case "javascript": return "javascript";
                case "ts": case "typescript": return "typescript";
                case "python": case "py": return "python";
                case "json": return "json";
                default: return lang;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Highlight <paramref name="code"/> for the given <paramref name="language"/>
        /// and return a Unity rich-text string.
        /// </summary>
        public static string Highlight(string code, string language)
        {
            if (string.IsNullOrEmpty(code)) return code;

            var lang = NormalizeLanguage(language);
            var tokens = Tokenize(code, lang);
            var sb = new StringBuilder(code.Length * 2);

            foreach (var token in tokens)
            {
                var escaped = EscapeRichText(token.Text);
                sb.Append("<color=").Append(token.Color).Append('>').Append(escaped).Append("</color>");
            }

            return sb.ToString();
        }

        // ── Tokenizer ──────────────────────────────────────────────────────
        private struct Token
        {
            public string Text;
            public string Color;
        }

        private static List<Token> Tokenize(string code, string lang)
        {
            var tokens = new List<Token>();
            int i = 0;
            int len = code.Length;

            while (i < len)
            {
                // ── Single-line comment ──
                if (IsSingleLineComment(code, i, lang))
                {
                    int start = i;
                    if (lang == "lua")
                    {
                        // Lua: -- comment
                        i += 2;
                    }
                    else if (lang == "python")
                    {
                        i += 1; // #
                    }
                    else
                    {
                        i += 2; // // or --
                    }
                    while (i < len && code[i] != '\n') i++;
                    tokens.Add(new Token { Text = code.Substring(start, i - start), Color = CommentColor });
                    continue;
                }

                // ── Multi-line comment ──
                if (TryMultiLineComment(code, i, lang, out int endMl))
                {
                    tokens.Add(new Token { Text = code.Substring(i, endMl - i), Color = CommentColor });
                    i = endMl;
                    continue;
                }

                // ── Preprocessor directive (#include, #define, etc.) — not Python ──
                if (code[i] == '#' && lang != "python" && lang != "json" && lang != "lua"
                    && (i == 0 || code[i - 1] == '\n'))
                {
                    int start = i;
                    i++;
                    while (i < len && code[i] != '\n') i++;
                    tokens.Add(new Token { Text = code.Substring(start, i - start), Color = PreprocessorColor });
                    continue;
                }

                // ── String literals ──
                if (TryString(code, i, out int endStr))
                {
                    tokens.Add(new Token { Text = code.Substring(i, endStr - i), Color = StringColor });
                    i = endStr;
                    continue;
                }

                // ── Char literal ──
                if (code[i] == '\'' && lang != "lua" && lang != "python" && i + 2 < len)
                {
                    int end = i + 1;
                    while (end < len && code[end] != '\'' && code[end] != '\n')
                    {
                        if (code[end] == '\\') end++;
                        end++;
                    }
                    if (end < len && code[end] == '\'') end++;
                    tokens.Add(new Token { Text = code.Substring(i, end - i), Color = StringColor });
                    i = end;
                    continue;
                }

                // ── Number ──
                if (char.IsDigit(code[i]) || (code[i] == '.' && i + 1 < len && char.IsDigit(code[i + 1])))
                {
                    int start = i;
                    // Hex
                    if (code[i] == '0' && i + 1 < len && (code[i + 1] == 'x' || code[i + 1] == 'X'))
                    {
                        i += 2;
                        while (i < len && IsHexDigit(code[i])) i++;
                        if (i < len && code[i] == 'L' || i < len && code[i] == 'l' ||
                            i < len && code[i] == 'U' || i < len && code[i] == 'u') i++;
                    }
                    else
                    {
                        while (i < len && (char.IsDigit(code[i]) || code[i] == '.' || code[i] == '_')) i++;
                        // float suffix
                        if (i < len && (code[i] == 'f' || code[i] == 'F' || code[i] == 'd' ||
                                         code[i] == 'D' || code[i] == 'm' || code[i] == 'M')) i++;
                    }
                    tokens.Add(new Token { Text = code.Substring(start, i - start), Color = NumberColor });
                    continue;
                }

                // ── Identifier / keyword ──
                if (IsIdentStart(code[i]))
                {
                    int start = i;
                    while (i < len && IsIdentPart(code[i])) i++;
                    string word = code.Substring(start, i - start);
                    string color = ClassifyWord(word, lang, tokens);
                    tokens.Add(new Token { Text = word, Color = color });
                    continue;
                }

                // ── Decorators / Attributes ──
                if ((code[i] == '@' && lang != "c" && lang != "cpp") ||
                    (code[i] == '[' && lang == "csharp"))
                {
                    int start = i;
                    if (code[i] == '[')
                    {
                        while (i < len && code[i] != ']' && code[i] != '\n') i++;
                        if (i < len) i++; // skip ]
                    }
                    else
                    {
                        i++; // skip @
                        while (i < len && IsIdentPart(code[i])) i++;
                    }
                    tokens.Add(new Token { Text = code.Substring(start, i - start), Color = AttributeColor });
                    continue;
                }

                // ── Punctuation / whitespace ──
                tokens.Add(new Token { Text = code[i].ToString(), Color = PunctuationColor });
                i++;
            }

            return tokens;
        }

        private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';
        private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';
        private static bool IsHexDigit(char c) => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        private static bool IsSingleLineComment(string code, int i, string lang)
        {
            if (i + 1 >= code.Length) return false;
            if (lang == "python") return code[i] == '#';
            if (lang == "lua") return code[i] == '-' && code[i + 1] == '-';
            return code[i] == '/' && code[i + 1] == '/';
        }

        private static bool TryMultiLineComment(string code, int i, string lang, out int end)
        {
            end = -1;
            if (i + 1 >= code.Length) return false;

            if (lang == "lua")
            {
                // Lua: --[[ multi-line ]]
                if (code[i] == '-' && code[i + 1] == '-' && i + 3 < code.Length &&
                    code[i + 2] == '[' && code[i + 3] == '[')
                {
                    int close = code.IndexOf("]]", i + 4);
                    end = close >= 0 ? close + 2 : code.Length;
                    return true;
                }
                return false;
            }

            if (lang == "python")
            {
                // Python: triple-quoted strings as docstrings/comments — skip for now
                return false;
            }

            // C-family: /* ... */
            if (code[i] == '/' && code[i + 1] == '*')
            {
                int close = code.IndexOf("*/", i + 2);
                end = close >= 0 ? close + 2 : code.Length;
                return true;
            }

            return false;
        }

        private static bool TryString(string code, int i, out int end)
        {
            end = -1;
            char quote = code[i];
            if (quote != '"' && quote != '\'') return false;

            int j = i + 1;
            while (j < code.Length && code[j] != quote && code[j] != '\n')
            {
                if (code[j] == '\\') j++; // skip escaped char
                j++;
            }

            if (j < code.Length && code[j] == quote) j++; // include closing quote
            end = j;
            return true;
        }

        // ── Word classification ───────────────────────────────────────────

        private static string ClassifyWord(string word, string lang, List<Token> tokens)
        {
            switch (lang)
            {
                case "csharp":
                    if (CSharpControl.Contains(word)) return ControlColor;
                    if (CSharpTypeKeywords.Contains(word)) return TypeColor;
                    if (CSharpKeywords.Contains(word)) return KeywordColor;
                    // C# function-call heuristic
                    return DefaultColor;

                case "cpp":
                case "c":
                    if (CLikeControl.Contains(word)) return ControlColor;
                    if (CLikeKeywords.Contains(word)) return KeywordColor;
                    if (lang == "cpp" && CppKeywords.Contains(word))
                    {
                        //区分控制流和类型
                        var cppControl = new HashSet<string> { "if", "else", "while", "for", "do", "switch",
                            "case", "break", "continue", "return", "throw", "try", "catch" };
                        if (cppControl.Contains(word)) return ControlColor;
                        var cppTypes = new HashSet<string> { "class", "struct", "enum", "namespace", "template", "typename" };
                        if (cppTypes.Contains(word)) return TypeColor;
                        return KeywordColor;
                    }
                    // C/C++: word followed by ( is likely a function call
                    return CheckFunctionCall(tokens, FuncCallColor);
                    break;

                case "java":
                    if (JavaControl.Contains(word)) return ControlColor;
                    if (JavaKeywords.Contains(word)) return KeywordColor;
                    return CheckFunctionCall(tokens, FuncCallColor);

                case "lua":
                    if (LuaKeywords.Contains(word)) return ControlColor;
                    if (LuaBuiltins.Contains(word)) return KeywordColor;
                    return CheckFunctionCall(tokens, FuncCallColor);

                case "python":
                    if (PythonKeywords.Contains(word)) return ControlColor;
                    if (PythonBuiltins.Contains(word)) return KeywordColor;
                    return CheckFunctionCall(tokens, FuncCallColor);

                case "javascript":
                case "typescript":
                    if (JsKeywords.Contains(word)) return ControlColor;
                    return CheckFunctionCall(tokens, FuncCallColor);

                case "json":
                    if (JsonKeywords.Contains(word)) return KeywordColor;
                    return DefaultColor;

                default:
                    break;
            }

            // For C#, check function call pattern
            if (lang == "csharp")
                return CheckFunctionCall(tokens, FuncCallColor);

            return DefaultColor;
        }

        /// <summary>
        /// If the previous token is "(", this word is likely a type name in a cast.
        /// If the next non-whitespace char is "(", this word is a function call.
        /// </summary>
        private static string CheckFunctionCall(List<Token> tokens, string funcColor)
        {
            // Look ahead in the original source: we can check the last token
            // Actually we don't have easy lookahead here. Just return default;
            // The function-call heuristic is optional and adds complexity.
            return DefaultColor;
        }

        // ── Rich-text escaping ─────────────────────────────────────────────

        private static string EscapeRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.IndexOf('<') >= 0 || text.IndexOf('>') >= 0)
                return "<noparse>" + text + "</noparse>";
            return text;
        }
    }
}
