using System;
using System.Collections.Generic;
using System.Net.Http;

namespace XRest.Core.Models
{
    public class ActionModel
    {
        public string Name { get; }
        public HttpMethod Method { get; }
        public string Path { get; }
        public IReadOnlyCollection<ParameterModel> Parameters { get; }
        public Type? Body { get; }
        public AuthModel Auth { get; }

        public ActionModel(
            string name,
            HttpMethod method,
            string path,
            IReadOnlyCollection<ParameterModel> parameters,
            Type? body,
            AuthModel auth
        )
        {
            Name = name;
            Method = method;
            Path = path;
            Parameters = parameters;
            Body = body;
            Auth = auth;
        }
    }
}