using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace gsi
{
    class BranchCli: Command 
	{	
        public bool ShowHelp;
        public string currentParameter;
        public List<string> create=new List<string>();
		public List<string> delete=new List<string>();
		public BranchCli () : base ("branch", "branch related functionality")
		{
			Options = new OptionSet () {
				"use as: gsi branch [OPTIONS]",
				"",
				{"?|h|help",
				"get help",
				v => ShowHelp = v != null },

				{"c|create", 
				"branches to create" , 
				v => currentParameter = "c"
            	},
				{"d|delete", 
				"branches to delete" , 
				v => currentParameter = "d"
            	},
				{ "<>", v => {
					switch(currentParameter) {
						case "c":
							create.Add(v);
							break;
						case "d":
							delete.Add(v);
							break;
					}}
				}
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse(args);
				if (create.Intersect(delete).Count()!=0)
					throw new Exception("ambiguity between created and deleted branches");
				GitCommand.BranchCmd(create,delete);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi branch: {e.Message}");
				Console.Error.WriteLine ($"gsi branch: aborted ...");
				return 1;
			}
		}
	}
}