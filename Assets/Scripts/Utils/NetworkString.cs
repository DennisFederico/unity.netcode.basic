using Unity.Collections;
using Unity.Netcode;

public struct NetworkString : INetworkSerializable
{
    private FixedString32Bytes payload;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref payload);
    }

    public override string ToString()
    {
        return payload.ToString();
    }

    public static implicit operator string(NetworkString s)
    {
        return s.ToString();
    }

    public static implicit operator NetworkString(string s)
    {
        return new() { payload = new FixedString32Bytes(s) };
    }
}