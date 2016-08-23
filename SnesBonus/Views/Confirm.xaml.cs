using System;
using System.Threading.Tasks;
using System.Windows;

namespace SnesBonus.Views {
	public partial class Confirm{
		public Confirm() {
			InitializeComponent();
			Deactivated += OnDeactivated;
		}

		public static Task<bool> AwaitResult() {
			return AwaitResult(
				double.NaN,
				double.NaN
			);
		}

		public static Task<bool> AwaitResult(Window owner) {
			var dialogue = new Confirm();
			dialogue.Left = owner.Left + owner.Width  / 2 - dialogue.Width  / 2;
			dialogue.Top  = owner.Top  + owner.Height / 2 - dialogue.Height / 2 - 50;
			dialogue.Show();
			return dialogue._task.Task;
		}

		public static Task<bool> AwaitResult(double x, double y) {
			var dialogue = new Confirm{
				Left = x,
				Top = y
			};
			dialogue.Show();
			return dialogue._task.Task;
		}

		private TaskCompletionSource<bool> _task = new TaskCompletionSource<bool>();

		private void OnDeactivated(object sender, EventArgs eventArgs) {
			Close();
		}

		private void Yes_Click(object sender, RoutedEventArgs e){
			_task.SetResult(true);
			_task = null;
			Close();
		}

		private void No_Click(object sender, RoutedEventArgs e){
			Close();
		}

		protected override void OnClosed(EventArgs e){
			if (_task != null){
				_task.SetResult(false);
				_task = null;
			}
			base.OnClosed(e);
		}
	}
}
