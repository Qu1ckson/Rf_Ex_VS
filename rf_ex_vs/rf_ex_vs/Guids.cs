// Guids.cs
// MUST match guids.h
using System;

namespace Qu1ckson.rf_ex_vs
{
    static class GuidList
    {
        public const string guidrf_ex_vsPkgString = "42b06706-80c0-4fd0-9c07-e2b6ab2708ea";
        public const string guidrf_ex_vsCmdSetString = "a6d3fa06-21e9-4a92-b444-9233360581c4";
        public const string guidToolWindowPersistanceString = "b7cd1bd3-8f20-4a6f-b3fa-13188d06e5e4";

        public static readonly Guid guidrf_ex_vsCmdSet = new Guid(guidrf_ex_vsCmdSetString);
    };
}