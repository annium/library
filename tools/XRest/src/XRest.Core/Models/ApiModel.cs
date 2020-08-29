using System;
using System.Collections.Generic;
using System.Reflection;

namespace XRest.Core.Models
{
    public class ApiModel
    {
        public Assembly Assembly
        {
            get => _assembly ?? throw new InvalidOperationException($"{nameof(ApiModel)}.{nameof(Assembly)} is not set");
            set
            {
                if (_assembly is null)
                    _assembly = value;
                else
                    throw new InvalidOperationException($"{nameof(ApiModel)}.{nameof(Assembly)} is already set");
            }
        }

        public IReadOnlyCollection<ControllerModel> Controllers { get; }
        private Assembly? _assembly;

        public ApiModel(
            IReadOnlyCollection<ControllerModel> controllers
        )
        {
            Controllers = controllers;
        }
    }
}