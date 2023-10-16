using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Server
{
	public class Category
	{

        [JsonPropertyName("cid")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set;  }

        public Category(int id, string name)
        {
            
            Id = id;
            Name = name;
        }
    }

   
}

