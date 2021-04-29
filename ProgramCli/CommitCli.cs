using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace gsi
{
    class CommitCli: Command 
	{
		public string Message;		
        public bool ShowHelp;
		public string currentParameter;
		public List<string> included=new List<string>();
		public List<string> excluded=new List<string>();
		
		public CommitCli () : base ("commit", "commit changes")
		{
			Options = new OptionSet () {
				"use as: gsi commit [OPTIONS]",
				"",
				{"m=|message=",
				"message",
				m => Message = m},
				{"?|h|help",
				"get help",
				v => ShowHelp = v != null },

				{"i|include", 
				"files to include" , 
				v => currentParameter = "i"
            	},
				{"e|exclude", 
				"files to exclude" , 
				v => currentParameter = "e"
            	},
				{ "<>", v => {
					switch(currentParameter) {
						case "i":
							included.Add(v);
							break;
						case "e":
							excluded.Add(v);
							break;
					}}
				}
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try 
			{
				var extra = Options.Parse (args);
				if (ShowHelp)
				{
					Options.WriteOptionDescriptions(CommandSet.Out);
					return 0;
				}
				if (included.Intersect(excluded).Count()!=0)
					throw new Exception("ambiguity between included and excluded files");
				GitCommand.CommitCmd(Message,included,excluded);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi commit: {e.Message}"); 
				Console.Error.WriteLine ($"gsi commit: aborted ...");
				return 1;
			}
		}

	}
}