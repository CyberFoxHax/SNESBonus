using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnesBonus.Views {
	public partial class ListEditor {
		private void RefreshGrid() {
			DataGrid.ItemsSource = null;
			DataGrid.ItemsSource = _games;
		}

		private void ReloadJsonFromDisk() {
			var newGames = Utils.LoadGamesFromJson();
			_games.Clear();
			_games.AddRange(newGames);
			RefreshGrid();
		}

		private static int Move<T>(IList<T> dest, T item, int offest) {
			var index = dest.IndexOf(item);
			var nextIndex = index + offest;
			var itemAtNext = dest.ElementAtOrDefault(nextIndex);
			if (itemAtNext == null)
				return -1;

			dest[index] = itemAtNext;
			dest[nextIndex] = item;

			return nextIndex;
		}
	}
}
