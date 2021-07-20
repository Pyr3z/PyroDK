// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(category:       "Style",
                           checkId:        "IDE0090:Use 'new(...)'",
                           Justification = "Unity C# does not support this syntax.")]

[assembly: SuppressMessage(category:       "Style",
                           checkId:        "IDE0083:Use pattern matching",
                           Justification = "Unity C# does not support pattern matching.")]

