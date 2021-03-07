using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Core.Network.Http.Native
{
	class PostData
	{
		private Dictionary<string, string> dictionaryPostData;
		private Dictionary<string, object> objectDictionaryPostData;

		public string Text { get; }
		public Dictionary<string, string> Dictionary
		{
			get
			{
				if (dictionaryPostData == null)
				{
					try
					{
						dictionaryPostData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Text);
					}
					catch (JsonReaderException)
					{
						dictionaryPostData = new Dictionary<string, string>();
					}
				}

				return dictionaryPostData;
			}
		}
		public Dictionary<string, object> ObjectDictionary
		{
			get
			{
				if (objectDictionaryPostData == null)
				{
					try
					{
						objectDictionaryPostData = JsonConvert.DeserializeObject<Dictionary<string, object>>(Text);
					}
					catch (JsonReaderException)
					{
						objectDictionaryPostData = new Dictionary<string, object>();
					}
				}

				return objectDictionaryPostData;
			}
		}

		public PostData()
		{
			Text = "";
		}
		public PostData(List<byte> data)
		{
			Text = Encoding.UTF8.GetString(data.ToArray());
		}
	}
}
