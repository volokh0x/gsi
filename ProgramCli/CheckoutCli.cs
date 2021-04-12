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
                    Console.WriteLine("gsi checkout: must specify one branch to checkout");
                    Console.WriteLine("gsi checkout: aborted ...");
					return 0;
                }
				GitCommand.CheckoutCmd(extra[0]);
				return 0;
			}
			catch (Exception e) 
			{
				Console.Error.WriteLine ($"gsi checkout: {e.ToString()}"); 
				return 1;
			}
		}

	}
}