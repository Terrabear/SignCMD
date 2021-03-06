﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SignCMD
{
	public class ScConfig
	{
		public string DefineSignCommands = "[sc]";
        public string CommandsStartWith = ">";
        public Dictionary<string, int> CooldownGroups = new Dictionary<string, int>();
		public bool ShowDestroyMessage = false;

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ScConfig Read(string path)
        {
            return !File.Exists(path) ? 
                new ScConfig()
                : JsonConvert.DeserializeObject<ScConfig>(File.ReadAllText(path));
        }
	}
}
