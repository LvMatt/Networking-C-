using System;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Server
{
	public class Category
	{
		public int Cid { get; set; }
		public string Name { get; set;  }

        public Category(int cid, string name)
        {
            Cid = cid;
            Name = name;
        }
    }

   
}

