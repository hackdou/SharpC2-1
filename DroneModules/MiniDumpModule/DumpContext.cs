namespace MiniDumpModule;

public class DumpContext
{
    public byte[] Data => _data;
    public uint Size { get; set; }
    public uint CurrentOffset { get; set; }

    private readonly uint _resizeIncrement;
    private byte[] _data;

    public DumpContext(uint resizeIncrement = 1024 * 1024 * 10)
    {
        _resizeIncrement = resizeIncrement;
        _data = Array.Empty<byte>();
    }

    public void Resize(uint newSize)
    {
        newSize = newSize - newSize % _resizeIncrement + _resizeIncrement;
        Array.Resize(ref _data, (int)newSize);
    }
}