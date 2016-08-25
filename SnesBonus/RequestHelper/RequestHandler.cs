using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SnesBonus.RequestHelper {
	public class RequestHandler<T>:RequestHandler{
		public RequestHandler(string url) : base(url){
			((RequestHandler)this).OnSuccess += handler => { if (OnSuccess != null) OnSuccess((RequestHandler<T>) handler); };
			((RequestHandler)this).OnError   += handler => { if (OnError   != null) OnError  ((RequestHandler<T>) handler); };
		}

		public new event Action<RequestHandler<T>> OnError;
		public new event Action<RequestHandler<T>> OnSuccess;

		public new RequestHandler<T> SendCallback() {
			base.SendCallback();
			return this;
		}

		[Obsolete]
		protected new RequestHandler<T> SendSync() {
			base.SendSync();
			return this;
		}

		public new async Task<RequestHandler<T>> SendAwait() {
			await base.SendAwait();
			return this;
		}

		public T Response(){
			return ResponseAs<T>();
		}
	}

	public class RequestHandler{
		public object AttachedData { get; set; }
		public string Url { get; set; }
		public RunningStatus RequestRunningStatus { get; private set; }
		public CompletionStatus RequestCompletedStatus { get; private set; }
		public HttpRequestMethod RequestMethod { get; set; }
		public HttpContentType ResponseContentType { get; set; }
		public Dictionary<string, string> QueryParams { get; set; }
		public Dictionary<string, string> RequestHeaders { get; set; }
		public Exception OnFailException { get; private set; }
		public PostData.IPostData PostData { get; set; }
		public System.Windows.Threading.Dispatcher Dispatcher { get; set; }
		public int Timeout { get; set; }

		public event Action<RequestHandler> OnSuccess;
		public event Action<RequestHandler> OnError;

		private Stream RawResponseStream { get; set; }
		private string RawResponseString{
			get{
				var str = new StreamReader(RawResponseStream).ReadToEnd();
				RawResponseStream.Dispose();
				var stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				writer.Write(str);
				writer.Flush();
				stream.Position = 0;
				RawResponseStream = stream;
				return str;
			}
		}

		private readonly TaskCompletionSource<RequestHandler> _task = new TaskCompletionSource<RequestHandler>();

		public T ResponseAs<T>(){
			if (RawResponseStream == null)
				return default(T);
			if (typeof (T) == typeof (String))
				return (T)(object)RawResponseString;

			switch (ResponseContentType){
				case HttpContentType.FormUrlEncoded:	return DeserializeUrlEncode <T>(RawResponseString);
				case HttpContentType.Json:				return DeserializeJsonEncode<T>(RawResponseString);
				case HttpContentType.Binary:			return (T)(object)RawResponseStream;
			}
			throw new Exception("Unable to find a method to deserialize data");
		}

		public static T DeserializeJsonEncode<T>(string input){
			return CsQuery.Utility.JSON.ParseJSONValue<T>(
				input
			);
		}

		private static T DeserializeUrlEncode<T>(string input){
			var dict = (
				from entry in input.Split('?','&')
				let eqSplit = entry.Split('=')
				select new{
					Key   = eqSplit[0],
					Value = eqSplit[1]
				}
			).ToDictionary(p=>p.Key, p=>p.Value);
			var outObj = Activator.CreateInstance<T>();
			foreach (var propertyInfo in typeof(T).GetProperties())
				propertyInfo.SetValue(outObj, dict[propertyInfo.Name], null);
			return outObj;
		}

		private static string UrlEncodeParameters(Dictionary<string, string> parameters) {
			return string.Join("&",parameters.Select(p=>p.Key+"="+p.Value));
		}

		public Task<RequestHandler> SendAwait(){
			new System.Threading.Thread(() =>{
				SendHttpRequest();
				_task.TrySetResult(this);
			}).Start();
			return _task.Task;
		}

		[Obsolete]
		protected RequestHandler SendSync() {
			SendHttpRequest();
			return this;
		}

		public RequestHandler SendCallback(){
			new System.Threading.Thread(SendHttpRequest).Start();
			return this;
		}

		public RequestHandler(string url){
			Url = url;
			QueryParams = new Dictionary<string, string>();
			RequestHeaders = new Dictionary<string, string>();
			ResponseContentType = HttpContentType.Json;
			Timeout = 10000;
		}

		private void SendHttpRequest(){
			var url = Url;
			if (QueryParams != null && QueryParams.Any())
				url += "?" + UrlEncodeParameters(QueryParams);

			var webReq = WebRequest.Create(url);
			webReq.Timeout = Timeout;
			webReq.Method = RequestMethod.ToString().ToUpper();
			foreach (var requestHeader in RequestHeaders)
				webReq.Headers.Add(requestHeader.Key, requestHeader.Value);

			if (PostData != null){
				if (new[]{
					HttpRequestMethod.Head,
					HttpRequestMethod.Get,
					HttpRequestMethod.Options,
					HttpRequestMethod.Trace
				}.Contains(RequestMethod))
					throw new Exception("You can't use the post data with the " + webReq.Method + " method");
				
				var postData = PostData.GetStream();
				postData.Position = 0;

				if (PostData.ContentType == null)
					throw new Exception("No content type is specified");

				webReq.ContentType = PostData.ContentType;
				webReq.ContentLength = postData.Length;

				try{
					var postDataContainer = webReq.GetRequestStream();
					postData.CopyTo(postDataContainer);
					postDataContainer.Close();
				}
				catch (Exception e){
					OnFailException = e;
					RequestCompletedStatus = CompletionStatus.Error;
				}

				postData.Close();
			}

			RequestRunningStatus = RunningStatus.Busy;
			HttpWebResponse webResp=null;
			try{
				var resp = webReq.GetResponse();
				webResp = (HttpWebResponse)resp;
			}
			catch (WebException e){
				webResp = (HttpWebResponse) e.Response;
				OnFailException = e;
				RequestCompletedStatus = CompletionStatus.Error;
			}
			catch (Exception e) {
				OnFailException = e;
				RequestCompletedStatus = CompletionStatus.Error;
			}
			if (RequestCompletedStatus != CompletionStatus.Error)
				RequestCompletedStatus = CompletionStatus.Success;
			RequestRunningStatus = RunningStatus.Completed;

			if (webResp != null)
				RawResponseStream = webResp.GetResponseStream();

			if (Dispatcher != null)
				Dispatcher.Invoke(InvokeEvents);
			else
				InvokeEvents();
		}

		private void InvokeEvents(){
			if (OnSuccess != null && RequestCompletedStatus == CompletionStatus.Success)
				OnSuccess(this);
			else if (OnError != null && RequestCompletedStatus == CompletionStatus.Error)
				OnError(this);
		}
	}
}
