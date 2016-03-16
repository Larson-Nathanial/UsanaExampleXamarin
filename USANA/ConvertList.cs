using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace USANA
{
	public class ConvertList
	{
		public ConvertList ()
		{
		}

		public string ConvertLToJ(List<Item> theList) 
		{
			return JsonConvert.SerializeObject(theList);
		}

		public List<Item> ConvertJTo(string theJson)
		{
			return JsonConvert.DeserializeObject<List<Item>> (theJson);
		}
	}
}

