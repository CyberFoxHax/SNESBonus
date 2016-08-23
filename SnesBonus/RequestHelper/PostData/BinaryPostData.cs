using System.IO;

namespace SnesBonus.RequestHelper.PostData{
	public class BinaryPostData : IPostData{
		public BinaryPostData() { }
		public BinaryPostData(Stream stream){
			PostDataStream = stream;
		}

		public Stream PostDataStream { get; set; }

		public Stream GetStream(){
			return PostDataStream;
		}


		public string ContentType {
			get{
				return "application/octet-stream";
			}
		}
	}
}