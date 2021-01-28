using System.Windows.Forms;

namespace SavescumBuddy.Wpf.Services
{
    public interface IGlobalKeyboardHook
    {
        bool HookActive { get; }
        bool Hook();
        void Unhook();
        event KeyEventHandler KeyDown;
        event KeyEventHandler KeyUp;
        event KeyPressEventHandler KeyPress;
    }
}
