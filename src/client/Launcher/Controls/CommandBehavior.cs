using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Controls;

public sealed class CommandBehavior : AvaloniaObject
{
    static CommandBehavior()
    {
        _ = CommandProperty.Changed.AddClassHandler<TextBlock>(HandleCommandChanged);
    }

    private static void HandleCommandChanged(TextBlock interactive, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is ICommand commandValue)
        {
            // Add non-null value
            interactive.AddHandler(InputElement.TappedEvent, Handler);
        }
        else
        {
            // remove prev value
            interactive.RemoveHandler(InputElement.TappedEvent, Handler);
        }

        // local handler fcn
        static void Handler(object? s, RoutedEventArgs e)
        {
            if (s is TextBlock tb)
            {
                // This is how we get the parameter off of the gui element.
                var commandParameter = tb.GetValue(CommandParameterProperty);
                var commandValue = tb.GetValue(CommandProperty);

                if (commandValue?.CanExecute(commandParameter) == true)
                {
                    commandValue.Execute(commandParameter);
                }
            }
        }
    }

    public static readonly AttachedProperty<ICommand> CommandProperty =
        AvaloniaProperty.RegisterAttached<CommandBehavior, TextBlock, ICommand>("Command");

    public static readonly AttachedProperty<object> CommandParameterProperty =
        AvaloniaProperty.RegisterAttached<CommandBehavior, TextBlock, object>("CommandParameter");

    public static void SetCommand(AvaloniaObject element, ICommand commandValue)
    {
        _ = element.SetValue(CommandProperty, commandValue);
    }

    public static ICommand GetCommand(AvaloniaObject element)
    {
        return element.GetValue(CommandProperty);
    }

    public static void SetCommandParameter(AvaloniaObject element, object parameter)
    {
        _ = element.SetValue(CommandParameterProperty, parameter);
    }

    public static object GetCommandParameter(AvaloniaObject element)
    {
        return element.GetValue(CommandParameterProperty);
    }
}
