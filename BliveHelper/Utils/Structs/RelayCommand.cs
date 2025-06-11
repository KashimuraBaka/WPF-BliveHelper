using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BliveHelper.Utils.Structs
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _asyncExecute;
        private readonly Action _syncExecute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        // 构造函数：支持同步方法
        public RelayCommand(Action execute, Func<object, bool> canExecute = null)
        {
            _syncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 构造函数：支持异步方法
        public RelayCommand(Func<Task> execute, Func<object, bool> canExecute = null)
        {
            _asyncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
        {
            if (_syncExecute != null)
            {
                _syncExecute();
            }
            else if (_asyncExecute != null)
            {
                _asyncExecute().ConfigureAwait(false);
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _asyncExecute;
        private readonly Action<T> _syncExecute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        // 构造函数：支持同步方法
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _syncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 构造函数：支持异步方法
        public RelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            _asyncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter)
        {
            if (_syncExecute != null)
            {
                _syncExecute((T)parameter);
            }
            else if (_asyncExecute != null)
            {
                _asyncExecute((T)parameter).ConfigureAwait(false);
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
