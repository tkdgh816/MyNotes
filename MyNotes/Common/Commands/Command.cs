using System.Windows.Input;

namespace MyNotes.Common.Commands;

public class Command : ICommand
{
  private readonly Action? _executeNoParams;
  private readonly Action<object>? _execute;
  private readonly Predicate<object>? _canExecute;

  public Command(Action execute, Predicate<object>? canExecute = null)
  {
    _executeNoParams = execute;
    _canExecute = canExecute;
  }

  public Command(Action<object> execute, Predicate<object>? canExecute = null)
  {
    _execute = execute;
    _canExecute = canExecute;
  }

  public Command(Action executeNoParams, Action<object> execute, Predicate<object>? canExecute = null)
  {
    _execute = execute;
    _executeNoParams = executeNoParams;
    _canExecute = canExecute;
  }

  public event EventHandler? CanExecuteChanged;

  public bool CanExecute(object? parameter)
  {
    if (parameter is null)
      return true;
    if (_canExecute is null)
      return true;

    return _canExecute(parameter);
  }

  public void Execute(object? parameter)
  {
    if (_execute is not null && parameter is not null)
      _execute(parameter);
    else if (_executeNoParams is not null)
      _executeNoParams();
  }
}

public class Command<T> : ICommand
{
  private readonly Action<T>? _execute;
  private readonly Predicate<T>? _canExecute;

  public Command(Action<T> execute, Predicate<T>? canExecute = null)
  {
    _execute = execute;
    _canExecute = canExecute;
  }

  public event EventHandler? CanExecuteChanged;

  public bool CanExecute(object? parameter)
  {
    if (parameter is null)
      return true;
    if (_canExecute is null)
      return true;

    return _canExecute((T)parameter);
  }

  public void Execute(object? parameter)
  {
    if (_execute is not null && parameter is not null)
      _execute((T)parameter);
  }
}