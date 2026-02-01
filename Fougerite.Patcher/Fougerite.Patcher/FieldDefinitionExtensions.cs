using System;
using Mono.Cecil;

namespace Fougerite.Patcher
{
    static class FieldDefinitionExtensions
    {
        public static FieldDefinition SetPublic(this FieldDefinition self, bool value)
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