using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace AnimeLibrary.View.UserControls
{
    public partial class SearchTextBox : UserControl, INotifyPropertyChanged
    {
        public TextBox Input => txtInput;

        public SearchTextBox()
        {
            DataContext = this;
            InitializeComponent();
            tbPlaceHolder.Visibility = Visibility.Visible;
        }

        public static event Action<string>? OnSearchChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private string placeholder;

        public string PlaceHolder
        {
            get { return placeholder; }
            set
            {
                placeholder = value;
                OnPropertyChanged();
            }
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                tbPlaceHolder.Visibility = Visibility.Visible;
            }
            else
            {
                tbPlaceHolder.Visibility = Visibility.Hidden;
            }

            OnSearchChanged?.Invoke(txtInput.Text);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
