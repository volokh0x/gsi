using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class AddCli: Command 
	{	
		public AddCli () : base ("add", "add files to staging area")
		{
			Options = new OptionSet () {
				"usage: gsi add <path1> [path2...]",
				"",
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse (args);
				if (extra.Count==0)
                {
					throw new Exception("nothing to add");
                }
				GitCommand.AddCmd(extra);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi add: {e.Message}"); 
				Console.Error.WriteLine ($"gsi add: aborted ...");
				return 1;
			}
		}
	}
}