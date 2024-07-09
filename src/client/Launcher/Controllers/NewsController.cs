// SPDX-License-Identifier: AGPL-3.0-or-later
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed class NewsController : ViewController
{
    public override MaterialIconKind IconKind => MaterialIconKind.Newspaper;

    public NewsController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }
}
