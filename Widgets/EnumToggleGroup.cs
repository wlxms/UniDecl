using System;
using System.Collections.Generic;
using UniDecl.Core;

namespace UniDecl.Widgets
{
    /// <summary>
    /// 枚举按钮组——每个枚举值一个等宽按钮，当前值高亮（[EnumToggleButtons] 的载体）。
    /// Value 为枚举底层 int，与 EnumField 一致，便于值同步与快照绑定。
    /// </summary>
    public class EnumToggleGroup : Element
    {
        public Type EnumType { get; set; }
        public int Value { get; set; }
        public Action<int> OnValueChanged { get; set; }

        public override IElement Render() => null;

        public EnumToggleGroup(Type enumType, int value = 0)
        {
            EnumType = enumType;
            Value = value;
        }

        public string[] GetChoiceNames()
        {
            if (EnumType == null || !EnumType.IsEnum) return Array.Empty<string>();
            return Enum.GetNames(EnumType);
        }

        public int IndexOfValue()
        {
            var names = GetChoiceNames();
            for (int i = 0; i < names.Length; i++)
                if (Convert.ToInt32(Enum.Parse(EnumType, names[i])) == Value)
                    return i;
            return -1;
        }

        public int ValueAtIndex(int index)
        {
            var names = GetChoiceNames();
            return index >= 0 && index < names.Length ? Convert.ToInt32(Enum.Parse(EnumType, names[index])) : Value;
        }
    }
}
