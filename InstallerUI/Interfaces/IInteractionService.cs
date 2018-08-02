using System;

namespace InstallerUI.Interfaces
{
    public interface IInteractionService
    {
        void CloseUIAndExit();
        void RunOnUIThread(Action body);
        IntPtr GetMainWindowHandle();
    }
}
