namespace SnesBonus.Models{
	public sealed class Game  {
		public string	Title			{ get; set; }
		public string	MetacriticScore	{ get; set; }
		public string	Publisher		{ get; set; }
		public string	ReleaseDate		{ get; set; }
		public string	ImagePath		{ get; set; }
		public string	Location		{ get; set; }
		public string	FilePath		{ get; set; }
		internal bool	IsScraped		{ get; set; }
		public string	Description		{ get; set; }
	}
}