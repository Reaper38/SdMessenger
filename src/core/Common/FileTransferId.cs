namespace Sdm.Core
{
    /// <summary>Unique file transfer session id (provided by server).</summary>
    public struct FileTransferId
    {
        public readonly uint Value;

        public FileTransferId(uint value)
        { Value = value; }

        public static bool operator ==(FileTransferId a, FileTransferId b)
        { return a.Value == b.Value; }

        public static bool operator !=(FileTransferId a, FileTransferId b)
        { return !(a == b); }

        public int CompareTo(FileTransferId other)
        { return Value.CompareTo(other.Value); }

        public bool Equals(FileTransferId other)
        { return Value == other.Value; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is FileTransferId && Equals((FileTransferId)obj);
        }

        public override int GetHashCode() { return Value.GetHashCode(); }

        public override string ToString() { return Value.ToString(); }
    }
}
