using System;
using Mono.Cecil;

namespace Fougerite.Patcher
{
    static class MethodDefinitionExtensions
    {
        public static MethodDefinition SetPublic(this MethodDefinition self, bool value)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            self.IsPublic = value;
            self.IsPrivate = !value;

            return self;
        }
    }
}