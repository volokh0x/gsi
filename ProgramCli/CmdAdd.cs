using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class CmdAdd: Command 
	{
		public List<string> Files;
		
		public CmdAdd () : base ("add", "add files to staging area")
		{
			Options = new OptionSet () {
				"usage: gsi add [files]",
				"",
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try {
				var extra = Options.Parse (args);
				if (extra.Count==0)
                {
                    Console.WriteLine("gsi add: nothing to add");
                    Console.WriteLine("gsi add: aborted ...");
					return 0;
                }
                Files=extra;
				GitCommand.Add(Files);
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine ($"gsi add: {e.ToString()}"); // e.Message
				return 1;
			}
		}
	}
}