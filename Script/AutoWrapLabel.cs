// using Godot;

// [GlobalClass]
// [Tool]
// public partial class AutoWrapLabel : Label
// {
//     [Export] public float MaxWidth { get; set; } = 300f;

//     public override void _Ready()
//     {
//         // 监听文本变化和尺寸变化，以便实时重新计算
//         ItemRectChanged += OnLayoutChanged;
//         // 初始时执行一次布局计算
//         CallDeferred(MethodName.UpdateLabelLayout);
//     }

//     // 当文本属性被修改时，手动触发重新布局
//     public override bool _Set(StringName property, Variant value)
//     {
//         if (property == "text")
//         {
//             CallDeferred(MethodName.UpdateLabelLayout);
//         }

//         return base._Set(property, value);
//     }



//     private void OnLayoutChanged()
//     {
//         UpdateLabelLayout();
//     }

//     private void UpdateLabelLayout()
//     {
//         if (!IsInsideTree() || string.IsNullOrEmpty(Text))
//         {
//             // 空文本时重置状态
//             AutowrapMode = TextServer.AutowrapMode.Off;
//             CustomMinimumSize = new Vector2(0, 0);
//             return;
//         }

//         var font = GetThemeFont("font");
//         int fontSize = GetThemeFontSize("font_size");
//         if (font == null) return;

//         // 1. 关闭换行，测量文本在“理想状态”下的真实宽度
//         AutowrapMode = TextServer.AutowrapMode.Off;
//         CustomMinimumSize = new Vector2(0, 0); // 清除最小宽度限制，让 Label 报告真实尺寸
        
//         // 使用 TextServer 直接测量，避免依赖 Label 的 Size（此时可能尚未更新）
//         Vector2 textSize = font.GetStringSize(Text, HorizontalAlignment.Left, -1, fontSize);

//         // 2. 根据真实宽度与上限的对比，决定最终状态
//         if (textSize.X > MaxWidth)
//         {
//             // 超长文本：开启智能换行，并锁定最大宽度
//             AutowrapMode = TextServer.AutowrapMode.WordSmart;
//             CustomMinimumSize = new Vector2(MaxWidth, 0);
//         }
//         else
//         {
//             // 短文本：保持不换行，让 Label 自然包裹内容
//             AutowrapMode = TextServer.AutowrapMode.Off;
//             CustomMinimumSize = new Vector2(0, 0);
//         }
//     }
// }