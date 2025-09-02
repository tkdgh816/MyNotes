using System.Windows.Input;

namespace MyNotes.Common.Commands;

internal class Command : ICommand
{
  private readonly Action? _execute;
  private readonly Func<bool>? _canExecute;

  private readonly Action<object>? _executeWithParam;
  private readonly Func<object, bool>? _canExecuteWithParam;

  public Command(Action execute, Func<bool>? canExecute = null)
  {
    _execute = execute;
    _canExecute = canExecute;
  }

  public Command(Action<object> execute, Func<object, bool>? canExecute = null)
  {
    _executeWithParam = execute;
    _canExecuteWithParam = canExecute;
  }

  public event EventHandler? CanExecuteChanged;

  public bool CanExecute(object? parameter = null)
    => parameter is null
      ? _canExecute is null || _canExecute()
      : _canExecuteWithParam is null || _canExecuteWithParam(parameter);

  public void Execute(object? parameter = null)
  {
    if (!CanExecute(parameter))
      return;

    if (parameter is null)
    {
      if (_execute is not null)
        _execute();
    }
    else
    {
      if (_executeWithParam is not null)
        _executeWithParam(parameter);
    }
  }
}

internal class Command<T> : ICommand
{
  private readonly Action<T>? _execute;
  private readonly Func<T, bool>? _canExecute;

  public Command(Action<T> execute, Func<T, bool>? canExecute = null)
  {
    _execute = execute;
    _canExecute = canExecute;
  }

  public event EventHandler? CanExecuteChanged;

  public bool CanExecute(object? parameter) 
    => parameter is null || _canExecute is null ? true : _canExecute((T)parameter);

  public void Execute(object? parameter)
  {
    if (!CanExecute(parameter))
      return;

    if (_execute is not null && parameter is not null)
      _execute((T)parameter);
  }
}