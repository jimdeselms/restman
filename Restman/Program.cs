using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Restman
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var commandLineArgs = CommandLine.Parse(args);
            var file = File.ReadAllText(commandLineArgs.SpecFile);
            var spec = JsonConvert.DeserializeObject<RestmanSpec>(file);

            var response = await RequestRunner.RunRequest(
                spec,
                commandLineArgs.Request, 
                commandLineArgs.VariableSets,
                commandLineArgs.AdditionalVariables,
                commandLineArgs.OrderedVariables,
                commandLineArgs.BifoqlQuery,
                commandLineArgs.Body);

            Console.WriteLine(response);
        }
    }
}
