using System;

namespace SavescumBuddy.Services.Interfaces
{
    public interface IHotkeyListener<TKey>
    {
        bool HookActive { get; }
        bool Hook();
        void Unhook();
        event EventHandler<IKeyEventArgs<TKey>> KeyDown;
        event EventHandler<IKeyEventArgs<TKey>> KeyUp;
        event EventHandler<IKeyPressEventArgs<TKey>> KeyPress;
    }

    public interface IKeyEventArgs<TKey>
    {
        TKey KeyCode { get; }
        bool SuppressKeyPress { get; }
        bool Handled { get; }
    }

    public interface IKeyPressEventArgs<TKey>
    {
        char KeyChar { get; }
        bool Handled { get; }
    }
}
