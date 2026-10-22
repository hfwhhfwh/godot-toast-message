using Godot;
using System;

public partial class World : Node
{
    private ToastManager _toastManager;

    public override void _Ready()
    {
        base._Ready();

        _toastManager = ToastManager.Instance;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);

        if(@event is InputEventKey  {Pressed: true, Echo: false} inputEventKey)
        {
            if(inputEventKey.Keycode == Key.Q)
            {
                _toastManager.ShowToast("操作成功", ToastColorPreset.OK);
            }
            else if(inputEventKey.Keycode == Key.W)
            {
                _toastManager.ShowToast("警告", ToastColorPreset.Warning);
            }
            else if(inputEventKey.Keycode == Key.E)
            {
                _toastManager.ShowToast("操作失败", ToastColorPreset.Error);
            }
        }
    }

}
