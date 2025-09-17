using System;

namespace Modding.API {
    [AttributeUsage(AttributeTargets.Class)]
    public class ModAPICategoryAttribute : Attribute {
        public string Category { get; }

        public ModAPICategoryAttribute(string category) {
            Category = category;
        }
    }
}