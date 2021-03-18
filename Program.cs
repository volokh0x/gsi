using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using Mono.Unix;
using Mono.Options;

namespace gsi
{
    static class Program
    {
        static int Main (string[] args)
        {
            var commands = new CommandSet ("gsi") {
                "usage: gsi COMMAND [OPTIONS]+",
                new InitCmd(),  
                new AddCmd()
            };
            return commands.Run(args);
        }
    }
}
