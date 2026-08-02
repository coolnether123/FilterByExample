using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FilterByExample.Domain
{
    internal sealed class ReferenceIdentityComparer :
        IEqualityComparer<object>
    {
        internal static readonly ReferenceIdentityComparer Instance =
            new ReferenceIdentityComparer();

        public new bool Equals(object left, object right)
        {
            return ReferenceEquals(left, right);
        }

        public int GetHashCode(object value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }
    }
}
