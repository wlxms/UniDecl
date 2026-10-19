using UniDecl.Core;

namespace UniDecl.Unity.Logging
{
    /// <summary>
    /// 将 UniDecl.Core 的日志入口桥接到 UnityEngine.Debug。
    /// 由 RuntimeInitializeOnLoadMethod（Player/PlayMode）与 UnityEditorBootstrap（EditMode）触发。
    /// </summary>
    public static class UnityLogBootstrap
    {
        private static bool _initialized;

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            UniDeclLog.Error = message => UnityEngine.Debug.LogError(message);
            UniDeclLog.Warning = message => UnityEngine.Debug.LogWarning(message);
            UniDeclLog.Info = message => UnityEngine.Debug.Log(message);
            UniDeclLog.Exception = (message, exception) =>
                UnityEngine.Debug.LogError($"{message}{System.Environment.NewLine}{exception}");
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeOnRuntime() => EnsureInitialized();
    }
}
