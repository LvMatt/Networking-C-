using System;
using System.Text.Json.Serialization;

namespace Server
{
	public class Request
	{



        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }


    
    }
}

