using UnityEditor;

namespace UniDecl.Unity.Editor
{
    /// <summary>
    /// EditMode 下的 Unity 适配初始化（日志桥接等）。
    /// Player/PlayMode 由 UnityLogBootstrap 的 RuntimeInitializeOnLoadMethod 处理。
    /// </summary>
    [InitializeOnLoad]
    internal static class UnityEditorBootstrap
    {
        static UnityEditorBootstrap()
        {
            UniDecl.Unity.Logging.UnityLogBootstrap.EnsureInitialized();
        }
    }
}
