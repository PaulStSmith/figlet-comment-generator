using System;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

/// <summary>
/// Service to display information bars in Visual Studio.
/// </summary>
public class InfoBarService : IInfoBarService
{
    private readonly IVsInfoBarUIFactory _infoBarUIFactory;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfoBarService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public InfoBarService(IServiceProvider serviceProvider)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        _serviceProvider = serviceProvider;
        _infoBarUIFactory = serviceProvider.GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
    }

    /// <summary>
    /// Shows an information bar with the specified message and optional actions.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="actions">The actions to include in the information bar.</param>
    public void ShowMessage(string message, IVsInfoBarActionItem[] actions = null)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var model = new InfoBarModel(
            [new InfoBarTextSpan(message)],
            actions ?? [],
            KnownMonikers.StatusInformation,
            isCloseButtonVisible: true);

        var element = _infoBarUIFactory.CreateInfoBar(model);

        if (_serviceProvider.GetService(typeof(SVsShell)) is IVsShell shell)
        {
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var host);
            var infoBarHost = host as IVsInfoBarHost;
            infoBarHost?.AddInfoBar(element);
        }
    }
}
