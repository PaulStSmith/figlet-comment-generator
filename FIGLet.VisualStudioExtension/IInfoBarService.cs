using Microsoft.VisualStudio.Shell.Interop;

namespace ByteForge.FIGLet.VisualStudioExtension;

/// <summary>
/// Interface for displaying information bars in Visual Studio.
/// </summary>
public interface IInfoBarService
{
    /// <summary>
    /// Shows an information bar with the specified message and optional actions.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="actions">The actions to include in the information bar.</param>
    void ShowMessage(string message, IVsInfoBarActionItem[] actions = null);
}
