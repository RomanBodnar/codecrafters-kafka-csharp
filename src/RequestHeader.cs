using System;

namespace codecrafters;

public struct RequestHeader {
    public short RequestApiKey;
    public short RequestApiVersion;
    public int CorrelationId;
    public NullableString ClientId;
    public CompactArray TagBuffer;

}
