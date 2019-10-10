using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bifoql;
using Newtonsoft.Json;

namespace Restman
{
    class RequestRunner
    {
        public static async Task<string> RunRequest(RestmanSpec spec, string requestName, IReadOnlyList<string> variableSets, IReadOnlyDictionary<string, string> variableOverrides, IReadOnlyList<string> orderedVariables, string bifoqlQuery)
        {
            var variables = new Dictionary<string, string>();
            if (spec.variableSets != null && spec.variableSets.ContainsKey("default"))
            {
                AddVariables(spec, "default", variables);
            }

            requestName = requestName ?? "default";
            var request = spec.requests[requestName];

            AddOrdinalVariables(request, orderedVariables, variables);
            foreach (var pair in variableOverrides)
            {
                variables[pair.Key] = pair.Value;
            }

            var url = SubstituteTokens(request.url, variables);

            var client = new HttpClient();
            var message = new HttpRequestMessage();
            message.RequestUri = new Uri(url);
            message.Method = new HttpMethod(request.method ?? "GET");
            if (request.headers != null)
            {
                foreach (var pair in request.headers)
                {
                    message.Headers.Add(SubstituteTokens(pair.Key, variables), SubstituteTokens(pair.Value, variables));
                }
            }

            var response = await client.SendAsync(message);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var builder = new System.Text.StringBuilder();
                builder.AppendLine("ERROR " + response.StatusCode);
                var rawContent = await response.Content.ReadAsStringAsync();
                builder.Append(Prettify(rawContent));
                return builder.ToString();
            }

            var content = await response.Content.ReadAsStringAsync();

            var query = bifoqlQuery ?? request.bifoqlQuery;
            if (query != null)
            {
                content = await RunBifoqlQuery(content, query);
            }

            return Prettify(content);
        }

        private static void AddOrdinalVariables(RequestSpec spec, IReadOnlyList<string> orderedVariables, Dictionary<string, string> variables)
        {
            if (spec.ordinalArgs != null)
            {
                for (int i = 0; i < spec.ordinalArgs.Count; i++)
                {
                    if (orderedVariables.Count > i)
                    {
                        variables[spec.ordinalArgs[i]] = orderedVariables[i];
                    }
                }
            }
        }

        private static string Prettify(string json)
        {
            // This might not actually be JSON, so don't fail if it isn't.

            try 
            {
                dynamic obj = JsonConvert.DeserializeObject(json);
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }

        private static async Task<string> RunBifoqlQuery(string content, string bifoqlQuery)
        {
            var jobj = JsonConvert.DeserializeObject<object>(content);
            var obj = ObjectConverter.ToAsyncObject(jobj);
            var query = Query.Compile(bifoqlQuery);
            var result = await query.Run(obj);

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        private static void AddVariables(RestmanSpec spec, string variableSet, Dictionary<string, string> variables)
        {
            var set = spec.variableSets[variableSet];
            foreach (var pair in set)
            {
                variables[pair.Key] = pair.Value;
            }
        }

        private static string SubstituteTokens(string text, IReadOnlyDictionary<string, string> variables)
        {
            var result = text;
            foreach (var pair in variables)
            {
                result = result.Replace("{{" + pair.Key + "}}", pair.Value);
            }

            return result;
        }
    }
}