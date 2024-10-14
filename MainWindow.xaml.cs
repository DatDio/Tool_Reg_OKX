using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tool_Reg_OKX.ViewModel;

namespace Tool_Reg_OKX
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainViewModel();
		}
		private void ApiKeyRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//var range = new TextRange(ApiKeyRichTextBox.Document.ContentStart, ApiKeyRichTextBox.Document.ContentEnd);
			//var viewModel = DataContext as MainViewModel;
			//if (viewModel != null)
			//{
			//	viewModel.ApiKeyWWProxy = range.Text.Trim(); // Cập nhật ApiKeyWWProxy trong ViewModel
			//}
		}
	}
}