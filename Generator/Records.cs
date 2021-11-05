using System.Collections.Generic;
using System.Collections.Immutable;

namespace FLRC.AgeGradeCalculator
{
    public static class Records
    {
        public static readonly IImmutableDictionary<Identifier, uint> All = new Dictionary<Identifier, uint>
        {
            //
        }.ToImmutableDictionary();
    }
}
