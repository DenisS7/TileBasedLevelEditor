using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.Commands
{
    public class RelayCommand : CommandBase
    {
        private readonly Action<object?> execute;
        private readonly Func<object?, bool>? canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            if (canExecute is not null)
                return canExecute(parameter);

            return base.CanExecute(parameter);
        }

        public override void Execute(object? parameter)
        {
            execute(parameter);
        }
    }
}
