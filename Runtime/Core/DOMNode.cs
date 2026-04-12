using System;
using System.Collections.Generic;

namespace UniDecl.Runtime.Core
{
    /// <summary>
    /// DOM 节点
    /// 由 BuildDOM 阶段展开元素树生成，Render 阶段遍历此结构进行实际渲染
    /// </summary>
    public class DOMNode
    {
        /// <summary>
        /// 对应的元素实例
        /// </summary>
        public IElement Element { get; set; }

        /// <summary>
        /// 元素的缓存状态
        /// </summary>
        public ElementState State { get; set; }

        /// <summary>
        /// 匹配到的渲染器
        /// </summary>
        public IElementRender Renderer { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<DOMNode> Children { get; } = new List<DOMNode>();

        /// <summary>
        /// 父节点
        /// </summary>
        public DOMNode Parent { get; set; }

        /// <summary>
        /// 是否有渲染器
        /// </summary>
        public bool HasRenderer => Renderer != null;

        /// <summary>
        /// 需要在 Render 阶段入栈的 Context（由 IContextProvider 设置）
        /// 非 null 时，RenderNode 遍历到此节点会 Push 到 ContextStack
        /// </summary>
        public IContextProvider ContextToPush { get; set; }

        /// <summary>
        /// ContextToPush 的类型，用于出栈时 Pop(Type)
        /// </summary>
        public Type ContextType { get; set; }
    }
}
