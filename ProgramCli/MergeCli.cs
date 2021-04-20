using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class MergeCli: Command 
	{	
		public MergeCli () : base ("merge", "merge with some branch")
		{
			Options = new OptionSet () {
				"usage: gsi branch [branch]",
				"",
			};
		}
		public override int Invoke (IEnumerable<string> args)
		{
			try 
			{
				var extra = Options.Parse (args);
				if (extra.Count!=1)
                {
                    Console.WriteLine("gsi merge: must specify one branch to checkout");
                    Console.WriteLine("gsi merge: aborted ...");
					return 0;
                }
				GitCommand.MergeCmd(extra[0]);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi merge: {e.ToString()}"); 
				return 1;
			}
		}

	}
}