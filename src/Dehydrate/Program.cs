using Microsoft.Framework.Runtime.Common.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dehydrate
{
    public class Program
    {
        public static int Main(string[] args) {
            var app = new CommandLineApplication
            {
                Name = "Dehydrate",
                Description = "Performs dehydration on a target assembly"
            };

            app.Command("dehydrate", c =>
            {
                c.Description = "Performs dehydration on the selected assembly.";

                var reverseOption = c.Option("-r|--reverse", "Display the message in reverse", CommandOptionType.NoValue);
                var messageArg = c.Argument("[message]", "The message you wish to display");
                c.HelpOption("-?|-h|--help");

                c.OnExecute(() =>
                {
                    var message = messageArg.Value;
                    if (reverseOption.HasValue()) {
                        message = new string(message.ToCharArray().Reverse().ToArray());
                    }
                    Console.WriteLine(message);
                    return 0;
                });
            });

            var targetAssemblies = app.Option("-a|--assemblies", "The target assembly to dehydrate", CommandOptionType.MultipleValue);
            var outputDirectory = app.Option("-o|--output", "Output directory for dehydrated assemblies", CommandOptionType.SingleValue);
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                if (!outputDirectory.HasValue()) {
                    Console.WriteLine("No output directory specified.");
                    return 1;
                }

                var dehydrator = new Dehydrator();
                dehydrator.Dehydrate(targetAssemblies.Values, Path.GetFullPath(outputDirectory.Value()));

                return 2;
            });

            return app.Execute(args);
        }
    }
}
