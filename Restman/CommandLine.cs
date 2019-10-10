using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Restman
{
    public class CommandLine
    {
        public string SpecFile { get; set; }
        public IReadOnlyDictionary<string, string> AdditionalVariables { get; set; }
        public IReadOnlyList<string> OrderedVariables { get; set; }
        public string Request { get; set; }

        public IReadOnlyList<string> VariableSets { get; set; }

        public string BifoqlQuery { get; set; }

        private CommandLine()
        {
        }

        public static CommandLine Parse(string[] args)
        {
            var cl = new CommandLine();
            cl.SpecFile = GetSpecFile(args);
            cl.Request = args.Length > 0 && !args[0].StartsWith('-') && args[0].IndexOf('=') == -1 ? args[0] : throw new Exception("Request name expected");
            cl.VariableSets = GetVariableSets(args);
            cl.OrderedVariables = GetOrderedVariables(args);
            cl.AdditionalVariables = GetAdditionalVariables(args);
            cl.BifoqlQuery = FindArg("-b", "--bifoql", args);

            return cl;
        }

        private static IReadOnlyList<string> GetVariableSets(string[] args)
        {
            var sets = FindArg("-v", "--variablesets", args);
            if (sets == null)
            {
                return new List<string>();
            }

            return sets.Split(',').Select(s => s.Trim()).ToList();
        }

        private static string GetSpecFile(string[] args)
        {
            var file = FindArg("-f", "--file", args);
            if (file != null)
            {
                return file;
            }
            else
            {
                if (File.Exists("./.restman.json"))
                {
                    return "./.restman.json";
                }

                var homeDirFile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.restman.json";

                if (File.Exists(homeDirFile))
                {
                    return homeDirFile;
                }

                throw new Exception("Please specify a Restman spec file.");
            }
        }

        private static IReadOnlyList<string> GetOrderedVariables(string[] args)
        {
            var list = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                if (!args[i].StartsWith('-') && args[i].IndexOf('=') == -1)
                {
                    list.Add(args[i]);
                }
            }

            return list;
        }

        private static IReadOnlyDictionary<string, string> GetAdditionalVariables(string[] args)
        {
            bool skipNext = false;
            var additionalVariables = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    skipNext = true;
                    continue;
                }

                if (skipNext) continue;

                skipNext = false;

                var equals = arg.IndexOf('=');
                if (equals == -1) continue;

                var key = arg.Substring(0, equals);
                var value = arg.Substring(equals+1);

                additionalVariables[key] = value;
            }

            return additionalVariables;
        }

        private static string FindArg(string shortName, string longName, string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == shortName || args[i] == longName)
                {
                    if (i+1 == args.Length)
                    {
                        throw new Exception("Missing argument " + shortName);
                    }
                    return args[i+1];
                }
            }

            return null;
        }

        public static void PrintUsage()
        {
        }
    }
}