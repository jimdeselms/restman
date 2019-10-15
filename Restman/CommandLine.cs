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

        public string Body { get; set; }

        private CommandLine()
        {
        }

        public static CommandLine Parse(string[] argArray)
        {
            var args = argArray.ToList();

            var cl = new CommandLine();
            // Put all the things that are defined by named options first
            cl.SpecFile = GetSpecFile(args);
            cl.VariableSets = GetVariableSets(args);
            cl.BifoqlQuery = FindArg("-q", "--query", args);
            var bodyFilename = FindArg("-b", "--body", args);
            if (bodyFilename != null)
            {
                cl.Body = File.ReadAllText(bodyFilename);
            }

            cl.AdditionalVariables = GetAdditionalVariables(args);
            
            // Now the only things left are things without "-" or "="
            cl.Request = args.Count > 0 ? args[0] : throw new Exception("Request name expected");
            args.RemoveAt(0);

            cl.OrderedVariables = args.ToArray();

            return cl;
        }

        private static IReadOnlyList<string> GetVariableSets(List<string> args)
        {
            var sets = FindArg("-v", "--variablesets", args);
            if (sets == null)
            {
                return new List<string>();
            }

            return sets.Split(',').Select(s => s.Trim()).ToList();
        }

        private static string GetSpecFile(List<string> args)
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

        private static IReadOnlyDictionary<string, string> GetAdditionalVariables(List<string> args)
        {
            var i = 0;
            var additionalVariables = new Dictionary<string, string>();

            while (i < args.Count)
            {
                var idx = args[i].IndexOf('=');
                if (idx != -1)
                {
                    var key = args[i].Substring(0, idx);
                    var value = args[i].Substring(idx+1);

                    additionalVariables[key] = value;

                    args.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return additionalVariables;
        }

        private static string FindArg(string shortName, string longName, List<string> args)
        {
            var index = args.IndexOf(shortName) != -1 ? args.IndexOf(shortName) : args.IndexOf(longName);
            if (index == -1 || index >= args.Count)
            {
                return null;
            }

            args.RemoveAt(index);
            var value = args[index];
            args.RemoveAt(index);

            return value;
        }
        private static bool FindSwitch(string shortName, string longName, List<string> args)
        {
            var index = args.IndexOf(shortName) != -1 ? args.IndexOf(shortName) : args.IndexOf(longName);
            if (index == -1 || index >= args.Count)
            {
                return false;
            }

            args.RemoveAt(index);
            return true;
        }

        public static void PrintUsage()
        {
        }
    }
}