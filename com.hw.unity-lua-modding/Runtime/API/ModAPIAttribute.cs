using System;
namespace Modding.API {
    [AttributeUsage(AttributeTargets.Method)]
    public class ModAPIAttribute : Attribute {
        public string APIName { get; }
        public string Description { get; }
        public bool IsAsync { get; }

        public ModAPIAttribute(string apiName, string description = "", bool isAsync = false) {
            APIName = apiName;
            Description = description;
            IsAsync = isAsync;
        }
    }
}