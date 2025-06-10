using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BliveHelper.Utils.Structs
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _asyncExecute;
        private readonly Action<object> _syncExecute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        // 构造函数：支持同步方法
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _syncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 构造函数：支持异步方法
        public RelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
        {
            _asyncExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
        {
            if (_syncExecute != null)
            {
                _syncExecute(parameter);
            }
            else if (_asyncExecute != null)
            {
                _asyncExecute(parameter).ConfigureAwait(false);
            }
        }
    }
}
