using System;
using System.Collections.Generic;

namespace UniDecl.BuiltIn.Runtime.Core
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

    public class DOMNode<TRenderResult> : DOMNode
    {
        private bool _hasRenderResult;
        private TRenderResult _renderResult;

        /// <summary>
        /// 该节点渲染结果在父容器 VE 中的顺序 index。
        /// 组织 VE 时由渲染宿主记录，重建替换 VE 时用于插回原位。
        /// </summary>
        public int RenderIndex { get; set; } = -1;

        public bool HasRenderResult => _hasRenderResult;

        public TRenderResult RenderResult
        {
            get => _renderResult;
            set
            {
                _renderResult = value;
                _hasRenderResult = true;
            }
        }

        public void ClearRenderResult()
        {
            _renderResult = default;
            _hasRenderResult = false;
        }
    }
}
