/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TWCore.Net.HttpServer
{
    /// <summary>
    /// Http url route handler
    /// </summary>
    public class RouteHandler
    {
        static string RouteParamPattern = @"({([0-9a-z?]*)*})";
        static Regex RouteParamRegex = new Regex(RouteParamPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static string RouteRequiredParamReplacePattern = @"([a-z0-9 -.$\[\]]+)";
        static string RouteOptionalParamReplacePattern = @"([a-z0-9 -.$\[\]]*)";

        string MatchRoute;
        List<string> RouteParameters = new List<string>();
        Regex rgxMatchRoute;

        #region Properties
        /// <summary>
        /// Http method
        /// </summary>
        public HttpMethod Method { get; private set; }
        /// <summary>
        /// Http route url
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Delegate of the request handler for this url
        /// </summary>
        public OnRequestHandler Handler { get; private set; }
        #endregion

        #region .ctor
        /// <summary>
        /// Http url handler
        /// </summary>
        /// <param name="method">Http method</param>
        /// <param name="url">Http route url</param>
        /// <param name="handler">Delegate of the request handler for this url</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RouteHandler(HttpMethod method, string url, OnRequestHandler handler)
        {
            Method = method;
            Url = url;
            Handler = handler;
            MatchRoute = url;

            foreach (Match match in RouteParamRegex.Matches(url))
            {
                if (match.Success && match.Value.Length > 0)
                {
                    RouteParameters.Add(match.Value);
                    if (match.Value.EndsWith("?}"))
                        MatchRoute = MatchRoute.Replace(match.Value, RouteOptionalParamReplacePattern);
                    else
                        MatchRoute = MatchRoute.Replace(match.Value, RouteRequiredParamReplacePattern);
                }
            }
            MatchRoute = MatchRoute.Replace("/", @"(\/*)");
            rgxMatchRoute = new Regex(MatchRoute, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Validate if a url match the route
        /// </summary>
        /// <param name="url">Url to test with the route</param>
        /// <returns>true if the url match the route; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Match(string url)
        {
            url = Uri.UnescapeDataString(url);
            var match = rgxMatchRoute.Match(url);
            return (match.Length == url.Length);
        }
        /// <summary>
        /// Extract the route parameters from the url
        /// </summary>
        /// <param name="url">Url to extract the parameters</param>
        /// <returns>Dictionary with the parameters for the route</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, string> GetRouteParameters(string url)
        {
            url = Uri.UnescapeDataString(url);
            var response = new Dictionary<string, string>();
            var match = rgxMatchRoute.Match(url);
            if (match.Length != url.Length)
                return null;

            var lstGroups = new List<Group>();
            for(var idx = 1; idx < match.Groups.Count; idx++)
            {
                if (match.Groups[idx].Value != "/")
                    lstGroups.Add(match.Groups[idx]);
            }

            var pIdx = 0;
            foreach(var group in lstGroups)
            {
                if (lstGroups.Count < RouteParameters.Count && RouteParameters[pIdx].EndsWith("?}"))
                    pIdx++;
                if (pIdx < RouteParameters.Count)
                {
                    var param = RouteParameters[pIdx];
                    var value = group.Value;
                    param = param.Substring(1, param.Length - 2).Replace("?", "");
                    response[param] = !string.IsNullOrEmpty(value) ? value : null;
                    pIdx++;
                }
            }
            return response;
        }
        #endregion
    }
}
