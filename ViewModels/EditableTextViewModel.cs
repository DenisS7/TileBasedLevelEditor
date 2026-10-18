using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Commands;
using TileBasedLevelEditor.ViewModel;

namespace TileBasedLevelEditor.ViewModels
{
    public class EditableTextViewModel : ViewModelBase
    {
        private string _oldText;
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
        }
        public RelayCommand StartEditCommand { get; }
        public RelayCommand ApplyEditCommand { get; }
        public RelayCommand ExitEditCommand { get; }

        public EditableTextViewModel(string text, Action<string> OnRename)
        {
            _oldText = text;
            _text = text;
            _isEditing = false;
            StartEditCommand = new RelayCommand(p => { IsEditing = true; });
            ExitEditCommand = new RelayCommand(p => { IsEditing = false; Text = _oldText; });
            ApplyEditCommand = new RelayCommand(p => { _oldText = Text; IsEditing = false; OnRename.Invoke(Text); });
        }
    }
}
