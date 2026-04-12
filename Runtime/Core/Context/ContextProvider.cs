using System.Collections;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// 上下文提供者基类
    /// 所有具体 Context 类继承此基类
    /// Context 不自行渲染，由 Manager 在渲染过程中控制入栈/出栈
    /// </summary>
    public abstract class ContextProvider : Element, IContextProvider
    {
        public IElement Child { get; private set; }

        public void Add(IElement child)
        {
            Child = child;
        }

        public override IElement Render()
        {
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            yield return Child;
        }
    }
}
