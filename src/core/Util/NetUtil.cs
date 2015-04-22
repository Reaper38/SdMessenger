using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Sdm.Core.Util
{
    public static class NetUtil
    {
        public static string GetIPStatusDesc(IPStatus status)
        {
            switch (status)
            {
            case IPStatus.BadDestination:
                return "Bad destination";
            case IPStatus.BadHeader:
                return "Bad header";
            case IPStatus.BadOption:
                return "Bad option";
            case IPStatus.BadRoute:
                return "Bad route";
            case IPStatus.DestinationHostUnreachable:
                return "Destination host unreachable";
            case IPStatus.DestinationNetworkUnreachable:
                return "Destination network unreachable";
            case IPStatus.DestinationPortUnreachable:
                return "Destination port unreachable";
            case IPStatus.DestinationProhibited:
                return "Destination prohibited";
            // equals to IPStatus.DestinationProhibited
            //case IPStatus.DestinationProtocolUnreachable:
            //    return "Destination protocol unreachable";
            case IPStatus.DestinationScopeMismatch:
                return "Destination scope mismatch";
            case IPStatus.DestinationUnreachable:
                return "Destination unreachable";
            case IPStatus.HardwareError:
                return "Hardware error";
            case IPStatus.IcmpError:
                return "ICMP error";
            case IPStatus.NoResources:
                return "Insufficient network resources";
            case IPStatus.PacketTooBig:
                return "Packet too big"; // mtu limit exceeded
            case IPStatus.ParameterProblem:
                return "Parameter problem";
            case IPStatus.SourceQuench:
                return "Source quench";
            case IPStatus.Success:
                return "Success";
            case IPStatus.TimedOut:
                return "Timed out";
            case IPStatus.TimeExceeded:
                return "Time exceeded";
            case IPStatus.TtlExpired:
                return "TTL expired";
            case IPStatus.TtlReassemblyTimeExceeded:
                return "TTL reassembly time exceeded";
            case IPStatus.Unknown:
                return "Unknown error";
            case IPStatus.UnrecognizedNextHeader:
                return "Unrecognized next header";
            default:
                return "Unknown error";
            }
        }
        
        public static string GetSocketErrorDesc(SocketError error)
        {
            switch (error)
            {
            case SocketError.AccessDenied:
                return "Access denied";
            case SocketError.AddressAlreadyInUse:
                return "Address already in use";
            case SocketError.AddressFamilyNotSupported:
                return "Address family not supported";
            case SocketError.AddressNotAvailable:
                return "Address not available";
            case SocketError.AlreadyInProgress:
                return "Already in progress";
            case SocketError.ConnectionAborted:
                return "Connection aborted";
            case SocketError.ConnectionRefused:
                return "Connection refused";
            case SocketError.ConnectionReset:
                return "Connection reset";
            case SocketError.DestinationAddressRequired:
                return "Destination address required";
            case SocketError.Disconnecting:
                return "Disconnecting";
            case SocketError.Fault:
                return "Fault";
            case SocketError.HostDown:
                return "Host down";
            case SocketError.HostNotFound:
                return "Host not found";
            case SocketError.HostUnreachable:
                return "Host unreachable";
            case SocketError.InProgress:
                return "Blocking call in progress";
            case SocketError.Interrupted:
                return "Blocking call cancelled";
            case SocketError.InvalidArgument:
                return "Invalid argument";
            case SocketError.IOPending:
                return "IO pending";
            case SocketError.IsConnected:
                return "Connected";
            case SocketError.MessageSize:
                return "Packet too big";
            case SocketError.NetworkDown:
                return "Network down";
            case SocketError.NetworkReset:
                return "Network reset";
            case SocketError.NetworkUnreachable:
                return "Network unreachable";
            case SocketError.NoBufferSpaceAvailable:
                return "Insufficient buffer";
            case SocketError.NoData:
                return "No data";
            case SocketError.NoRecovery:
                return "No recovery";
            case SocketError.NotConnected:
                return "Not connected";
            case SocketError.NotInitialized:
                return "Not initialized";
            case SocketError.NotSocket:
                return "Invalid operation";
            case SocketError.OperationAborted:
                return "Operation aborted";
            case SocketError.OperationNotSupported:
                return "Operation not supported";
            case SocketError.ProcessLimit:
                return "Process limit exceeded";
            case SocketError.ProtocolFamilyNotSupported:
                return "Protocol family not supported";
            case SocketError.ProtocolNotSupported:
                return "Protocol not supported";
            case SocketError.ProtocolOption:
                return "Invalid protocol option";
            case SocketError.ProtocolType:
                return "Invalid protocol type";
            case SocketError.Shutdown:
                return "Socket closed";
            case SocketError.SocketError:
                return "Unknown error";
            case SocketError.SocketNotSupported:
                return "Unsupported socket type";
            case SocketError.Success:
                return "Success";
            case SocketError.SystemNotReady:
                return "Network subsystem not available";
            case SocketError.TimedOut:
                return "Timed out";
            case SocketError.TooManyOpenSockets:
                return "Open sockets limit exceeded";
            case SocketError.TryAgain:
                return "Host name could not be resolved";
            case SocketError.TypeNotFound:
                return "Type not found";
            case SocketError.VersionNotSupported:
                return "Unsupported version";
            case SocketError.WouldBlock:
                return "Nonblocking call pending";
            default:
                return "Unknown error";
            }
        }

        /// <summary>Check if the base exception is SocketException with ConnectionReset error code.</summary>
        public static bool CheckConnectionReset(Exception e)
        {
            var se = e.GetBaseException() as SocketException;
            return se != null && se.SocketErrorCode == SocketError.ConnectionReset;
        }
    }
}
