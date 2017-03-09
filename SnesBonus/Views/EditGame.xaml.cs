using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnesBonus.Views {
	public partial class EditGame{
		public EditGame() {
			InitializeComponent();
		}

		private Models.Game _game;
		private static readonly Dictionary<Models.Game, EditGame> Editors = new Dictionary<Models.Game, EditGame>();

		public event Action<Models.Game> GameSaved;

		public static EditGame GetInstance(Models.Game game){
			if (Editors.ContainsKey(game)){
				Editors[game].Focus();
				return Editors[game];
			}

			var view = Editors[game] = new EditGame{ _game = game };
			view.ModelToView(game);
			view.Show();
			return view;
		}

		private void ModelToView(Models.Game game){
            if(game.ImagePath != null)
			    Image.Source = new BitmapImage(new Uri(game.FullImagePath));

			Title						= game.Title;
			TxtTitle			.Text	= game.Title			;
			TxtMetacriticScore	.Text	= game.MetacriticScore	;
			TxtPublisher		.Text	= game.Publisher		;
			TxtReleaseDate		.Text	= game.ReleaseDate		;
			TxtImagePath		.Text	= game.ImagePath		;
			TxtLocation			.Text	= game.Location			;
			TxtFilePath			.Text	= game.FilePath			;
			TxtDescription		.Text	= game.Description		;
		}

		private Models.Game ViewToModel(Models.Game game){
			game.Title = TxtTitle.Text;
			game.MetacriticScore = TxtMetacriticScore.Text;
			game.Publisher = TxtPublisher.Text;
			game.ReleaseDate = TxtReleaseDate.Text;
			game.ImagePath = TxtImagePath.Text;
			game.Location = TxtLocation.Text;
			game.FilePath = TxtFilePath.Text;
			game.Description = TxtDescription.Text;
			return game;
		}

		private void OnSave_Click(object sender, RoutedEventArgs e){
			ViewToModel(_game);

			if (GameSaved != null)
				GameSaved(_game);
			Close();
		}

		private void OnClosed_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		protected override void OnClosed(EventArgs e){
			Editors.Remove(_game);
			base.OnClosed(e);
		}
	}
}
