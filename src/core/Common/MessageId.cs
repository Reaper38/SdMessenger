namespace Sdm.Core
{
    // Cl : client to server
    // Sv : server to client
    public enum MessageId : ushort
    {
        SvPublicKeyChallenge = 0,
        ClPublicKeyRespond = 1,
        SvAuthChallenge = 2,
        ClAuthRespond = 3,
        SvAuthResult = 4,
        ClDisconnect = 5,
        SvDisconnect = 6,
        ClUserlistRequest = 7,
        SvUserlistRespond = 8,
    }
}
