using System;
using System.Diagnostics;

namespace UniDecl.Core
{
    /// <summary>
    /// 引擎无关日志入口。纯 C# 程序集不得引用 UnityEngine.Debug / UnityEditor。
    /// Unity 适配层通过 UnityLogBootstrap 注入实际日志实现；未注入时退化为 Trace/Console。
    /// </summary>
    public static class UniDeclLog
    {
        public static Action<string> Error { get; set; } = message => Trace.TraceError(message);
        public static Action<string> Warning { get; set; } = message => Trace.TraceWarning(message);
        public static Action<string> Info { get; set; } = message => Trace.TraceInformation(message);
        public static Action<string, Exception> Exception { get; set; } =
            (message, exception) => Trace.TraceError($"{message}{Environment.NewLine}{exception}");
    }
}
