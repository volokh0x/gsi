using System;
using System.Collections.Generic;
using Mono.Options;

namespace gsi
{
    class CheckoutCli: Command 
	{	
		public CheckoutCli () : base ("checkout", "checkout other branch")
		{
			Options = new OptionSet () {
				"usage: gsi checkout [branch]",
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
					throw new Exception("must specify one branch to checkout");
                }
				GitCommand.CheckoutCmd(extra[0]);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi checkout: {e.Message}"); 
				Console.Error.WriteLine ($"gsi checkout: aborted ...");
				return 1;
			}
		}

	}
}