// SPDX-License-Identifier: AGPL-3.0-or-later
using System.ComponentModel;
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal abstract partial class ViewController : ObservableValidator
{
    protected MainController MainController { get; }

    public IServiceProvider Services { get; }

    public abstract MaterialIconKind IconKind { get; }

    [ObservableProperty]
    private bool _isActive;

    protected ViewController(IServiceProvider services, MainController mainController)
    {
        MainController = mainController;
        Services = services;
        MainController.PropertyChanged += OnMainControllerPropertyChanged;
    }

    private void OnMainControllerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainController.CurrentContent))
        {
            IsActive = MainController.CurrentContent == this;
        }
    }

    [RelayCommand]
    protected void Activate()
    {
        MainController.CurrentContent = this;
    }
}
