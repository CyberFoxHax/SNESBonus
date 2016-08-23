
namespace SnesBonus {
	public partial class App{
#if DEBUG
		public App(){
			SnesBonus.Properties.Settings.Default.Reset();
		}
#endif
	}
}
