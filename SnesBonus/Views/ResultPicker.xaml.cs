using System;
using System.Collections.Generic;
using System.Windows.Input;
using T = SnesBonus.Scraper.SearchResult;

namespace SnesBonus.Views {
	public partial class ResultPicker{
		public ResultPicker() {
			InitializeComponent();
		}

		public event Action<T> OnPick;

		private System.Threading.Tasks.TaskCompletionSource<T> _task;

		public static System.Threading.Tasks.Task<T> AwaitPick(IEnumerable<T> source) {
			var instance = new ResultPicker {
				ListView = {
					ItemsSource = source
				},
				_task = new System.Threading.Tasks.TaskCompletionSource<T>()
			};
			instance.OnPick += s => instance._task.SetResult(s);
			instance.Closed += (sender, args) =>{
				if(instance._task.Task.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
					instance._task.SetResult(null);
			};
			instance.ShowDialog();
			return instance._task.Task;
		}

		public static ResultPicker GetInstance(IEnumerable<T> source) {
			var instance = new ResultPicker{
				ListView = {
					ItemsSource = source
				}
			};
			instance.Show();
			return instance;
		}

		private void ListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e){
			var selection = ListView.SelectedItem as T;
			if (selection == null) return;

			if (OnPick != null)  OnPick(selection);
			Close();
		}
	}
}
