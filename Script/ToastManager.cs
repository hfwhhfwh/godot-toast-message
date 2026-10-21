using Godot;
using System;
using System.Collections.Generic;

public partial class ToastManager : Node
{
    private const string ToastScenePath = "res://Scene/toast_item.tscn";
    private const double DisplayDuration = 3.0;
    private const float RightMargin = 20.0f;
    private const float TopMargin = 20.0f;
    private const float ToastSpacing = 10.0f;
    private const double AnimationDuration = 0.32;

    private readonly List<ToastEntry> _toasts = new();
    private PackedScene _toastScene;
    private Control _toastLayer;

    public static ToastManager Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        _toastScene = GD.Load<PackedScene>(ToastScenePath);

        var canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);

        _toastLayer = new Control
        {
            Name = "ToastContainer",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _toastLayer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        canvasLayer.AddChild(_toastLayer);

        GetViewport().SizeChanged += OnViewportSizeChanged;

        Instance = this;
    }

    public override void _ExitTree()
    {
        Instance = null;

        base._ExitTree();
    }


    public void ShowToast(string text)
    {
        if (_toastScene == null || _toastLayer == null)
        {
            GD.PushError("ToastManager could not load the toast scene.");
            return;
        }

        Control toast = _toastScene.Instantiate<Control>();
        toast.Set("text", text);
        _toastLayer.AddChild(toast);

        var entry = new ToastEntry(toast);
        _toasts.Add(entry);

        var viewportWidth = GetViewport().GetVisibleRect().Size.X;
        toast.Position = new Vector2(viewportWidth + RightMargin, TopMargin);
        PrepareAndShowToast(entry);
    }

    private async void PrepareAndShowToast(ToastEntry entry)
    {
        // 这里等待两个 ProcessFrame，目的是让 Godot 完成节点布局和最小尺寸计算
        // 因为刚 AddChild 时，文本尺寸可能还没更新
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (!IsInstanceValid(entry.Toast))
        {
            return;
        }

        entry.Toast.Size = entry.Toast.GetCombinedMinimumSize();
        entry.IsMeasured = true;
        ReflowToasts();

        AnimateToX(entry, GetToastX(entry.Toast), true);
    }

    private async void ExpireToast(ToastEntry entry)
    {
        await ToSignal(GetTree().CreateTimer(DisplayDuration), SceneTreeTimer.SignalName.Timeout);

        if (!IsInstanceValid(entry.Toast) || entry.IsExiting)
        {
            return;
        }

        entry.IsExiting = true;
        _toasts.Remove(entry);
        ReflowToasts();

        AnimateToX(entry, GetViewport().GetVisibleRect().Size.X + RightMargin);

        await ToSignal(entry.HorizontalTween, Tween.SignalName.Finished);
        if (IsInstanceValid(entry.Toast))
        {
            entry.Toast.QueueFree();
        }
    }

    private void ReflowToasts()
    {
        var targetY = TopMargin;
        foreach (var entry in _toasts)
        {
            var toastHeight = entry.IsMeasured
                ? entry.Toast.Size.Y
                : entry.Toast.GetCombinedMinimumSize().Y;
            if (toastHeight <= 0.0f)
            {
                toastHeight = 55.0f;
            }

            if (entry.IsEntered)
            {
                entry.VerticalTween?.Kill();
                entry.VerticalTween = CreateTween()
                    .SetTrans(Tween.TransitionType.Cubic)
                    .SetEase(Tween.EaseType.Out);
                entry.VerticalTween.TweenProperty(entry.Toast, "position:y", targetY, AnimationDuration);
            }
            else
            {
                entry.Toast.Position = new Vector2(entry.Toast.Position.X, targetY);
            }

            targetY += toastHeight + ToastSpacing;
        }
    }

    private float GetToastX(Control toast)
    {
        return GetViewport().GetVisibleRect().Size.X - RightMargin - toast.Size.X;
    }

    private void OnViewportSizeChanged()
    {
        foreach (var entry in _toasts)
        {
            if (!entry.IsMeasured || !IsInstanceValid(entry.Toast))
            {
                continue;
            }

            AnimateToX(entry, GetToastX(entry.Toast), !entry.IsEntered);
        }

        ReflowToasts();
    }

    private void AnimateToX(ToastEntry entry, float targetX, bool markEntered = false)
    {
        entry.HorizontalTween?.Kill();
        entry.HorizontalTween = CreateTween()
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        entry.HorizontalTween.TweenProperty(entry.Toast, "position:x", targetX, AnimationDuration);

        if (markEntered)
        {
            entry.HorizontalTween.TweenCallback(Callable.From(() =>
            {
                if (!IsInstanceValid(entry.Toast) || entry.IsExiting)
                {
                    return;
                }

                entry.IsEntered = true;
                ExpireToast(entry);
            }));
        }
    }

    private sealed class ToastEntry
    {
        public Control Toast { get; }
        public bool IsMeasured { get; set; }
        public bool IsEntered { get; set; }
        public bool IsExiting { get; set; }
        public Tween HorizontalTween { get; set; }
        public Tween VerticalTween { get; set; }

        public ToastEntry(Control toast)
        {
            Toast = toast;
        }
    }
}
