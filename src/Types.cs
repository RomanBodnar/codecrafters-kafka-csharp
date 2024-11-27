using System;

namespace codecrafters;

public struct NullableString {
    public short Length;
    /// <summary>
    /// UTF8
    /// </summary>
    public string? Value;
}

public struct CompactArray {
    public int Length;
}
