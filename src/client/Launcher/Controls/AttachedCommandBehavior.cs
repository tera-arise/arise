using System.Data;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Arise.Client.Launcher.Controls;

public static class AttachedCommandBehavior
{
    public static readonly AttachedProperty<ICommand> CommandProperty =
        AvaloniaProperty.RegisterAttached<CommandBehavior, Interactive, ICommand>("Command");

    public static readonly AttachedProperty<object> CommandParameterProperty =
        AvaloniaProperty.RegisterAttached<CommandBehavior, Interactive, object>("CommandParameter");

    public static readonly AttachedProperty<AttachedCommandTrigger> TriggerProperty =
        AvaloniaProperty.RegisterAttached<CommandBehavior, Interactive, AttachedCommandTrigger>("Trigger", AttachedCommandTrigger.Click);

    static AttachedCommandBehavior()
    {
        _ = CommandProperty.Changed.AddClassHandler<Interactive>(HandleCommandChanged);
        _ = TriggerProperty.Changed.AddClassHandler<Interactive>(HandleTriggerChanged);
    }

    private static void HandleTriggerChanged(Interactive interactive, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is AttachedCommandTrigger oldTrigger)
        {
            RemoveHandlers(interactive, oldTrigger);
        }

        if (args.NewValue is AttachedCommandTrigger newTrigger)
        {
            AddHandlers(interactive, newTrigger);
        }
    }

    private static void HandleCommandChanged(Interactive interactive, AvaloniaPropertyChangedEventArgs args)
    {
        var trigger = interactive.GetValue(TriggerProperty);

        if (args.NewValue is ICommand)
        {
            // Add non-null value
            AddHandlers(interactive, trigger);
        }
        else
        {
            // remove prev value
            RemoveHandlers(interactive, trigger);
        }
    }

    private static void AddHandlers(Interactive interactive, AttachedCommandTrigger oldTrigger)
    {
        switch (oldTrigger)
        {
            case AttachedCommandTrigger.Enter:
                interactive.AddHandler(InputElement.KeyDownEvent, EnterHandler);
                break;
            case AttachedCommandTrigger.Click:
                interactive.AddHandler(InputElement.TappedEvent, TappedHandler);
                break;
            case AttachedCommandTrigger.Both:
                interactive.AddHandler(InputElement.TappedEvent, TappedHandler);
                interactive.AddHandler(InputElement.KeyDownEvent, EnterHandler);
                break;
        }
    }

    private static void RemoveHandlers(Interactive interactive, AttachedCommandTrigger trigger)
    {
        switch (trigger)
        {
            case AttachedCommandTrigger.Enter:
                interactive.RemoveHandler(InputElement.KeyDownEvent, EnterHandler);
                break;
            case AttachedCommandTrigger.Click:
                interactive.RemoveHandler(InputElement.TappedEvent, TappedHandler);
                break;
            case AttachedCommandTrigger.Both:
                interactive.RemoveHandler(InputElement.TappedEvent, TappedHandler);
                interactive.RemoveHandler(InputElement.KeyDownEvent, EnterHandler);
                break;
        }
    }

    private static void EnterHandler(object? s, KeyEventArgs e)
    {
        if (s is Interactive i && e.Key is Key.Enter)
        {
            Execute(i);
        }
    }

    private static void TappedHandler(object? s, TappedEventArgs e)
    {
        if (s is Interactive i)
        {
            Execute(i);
        }
    }

    private static void Execute(AvaloniaObject obj)
    {
        // This is how we get the parameter off of the gui element.
        var commandParameter = obj.GetValue(CommandParameterProperty);
        var commandValue = obj.GetValue(CommandProperty);

        if (commandValue?.CanExecute(commandParameter) == true)
        {
            commandValue.Execute(commandParameter);
        }
    }

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

    public static void SetTrigger(AvaloniaObject element, AttachedCommandTrigger trigger)
    {
        _ = element.SetValue(TriggerProperty, trigger);
    }

    public static AttachedCommandTrigger GetTrigger(AvaloniaObject element)
    {
        return element.GetValue(TriggerProperty);
    }
}
