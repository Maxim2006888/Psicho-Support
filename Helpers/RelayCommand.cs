using System.Windows.Input;
using System;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action execute)
    {
        _execute = _ => execute();
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        _execute = _ => execute();
        _canExecute = _ => canExecute();
    }

    public RelayCommand(Action<object> execute)
    {
        _execute = execute;
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}