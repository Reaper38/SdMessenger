using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdm.Core;
using Sdm.Core.Util;

namespace Sdm.Server
{
    internal sealed class CommandManager
    {
        private static bool ParseAccessFlags(string s, out ClientAccessFlags flags)
        {
            // 0=none r=read w=write x=admin a=all
            flags = ClientAccessFlags.Default;
            foreach (char f in s)
            {
                switch (f)
                {
                case '0': continue;
                case 'r': flags |= ClientAccessFlags.Receive; continue;
                case 'w': flags |= ClientAccessFlags.Send; continue;
                case 'x': flags |= ClientAccessFlags.Admin; continue;
                case 'a': flags |= ClientAccessFlags.Max; continue;
                default: return false;
                }
            }
            return true;
        }

        private static string FormatAccessFlags(ClientAccessFlags flags)
        {
            if (flags == ClientAccessFlags.Default)
                return "0";
            if (flags == ClientAccessFlags.Max)
                return "a";
            var sb = new StringBuilder(16);
            if ((flags & ClientAccessFlags.Receive) == ClientAccessFlags.Receive)
                sb.Append('r');
            if ((flags & ClientAccessFlags.Send) == ClientAccessFlags.Send)
                sb.Append('w');
            if ((flags & ClientAccessFlags.Admin) == ClientAccessFlags.Admin)
                sb.Append('x');
            return sb.ToString();
        }

        private abstract class Command
        {
            public abstract string Name { get; }
            public abstract string Usage { get; }
            
            public void PrintUsage(PipeConsole console)
            { console.WriteLine("usage: {0} {1} {2}\r\n", Root.AppName, Name, Usage); }

            public abstract void Run(string[] args, PipeConsole console);
        }
        
        private sealed class CmdUserAdd : Command
        {
            public override string Name { get { return "user.add"; } }
            public override string Usage { get { return "<login> <pass> <access>"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (args.Length < 4)
                {
                    PrintUsage(console);
                    return;
                }
                var login = args[1]; // XXX: validate login
                var pass = args[2]; // XXX: validate password
                ClientAccessFlags access;
                if (!ParseAccessFlags(args[3], out access))
                {
                    console.WriteLine(Root.AppName + ": invalid user access flags.");
                    console.WriteLine("valid flags:\r\n" +
                        "    0=none\r\n" +
                        "    r=read\r\n" +
                        "    w=write\r\n" +
                        "    x=admin\r\n" +
                        "    a=all");
                    return;
                }
                var pswHash = UserAccount.TransformPassword(pass);
                var acc = new UserAccount(login, pswHash, access);
                if (Root.Server.Users.AddUser(acc))
                    console.WriteLine("created user " + login);
                else
                    console.WriteLine("user already exists");
            }
        }

        private sealed class CmdUserSt : Command
        {
            public override string Name { get { return "user.st"; } }
            public override string Usage { get { return "[login]"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (args.Length == 1)
                {
                    // XXX: show overall userlist info
                    console.WriteLine("no info");
                    return;
                }
                if (args.Length >= 2)
                {
                    var users = Root.Server.Users;
                    var login = args[1];
                    var user = users.FindUser(login);
                    if (user == null)
                    {
                        console.WriteLine("user not found");
                        return;
                    }
                    console.WriteLine("user {0} : {1}", user.Login, FormatAccessFlags(user.Access));
                }
            }
        }

        private sealed class CmdUserDel : Command
        {
            public override string Name { get { return "user.del"; } }
            public override string Usage { get { return "{<login>|--all}"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (args.Length < 2)
                {
                    PrintUsage(console);
                    return;
                }
                var users = Root.Server.Users;
                var login = args[1];
                if (login == "--all")
                {
                    // XXX: delete all users
                    console.WriteLine("not implemented yet");
                    return;
                }
                if (users.RemoveUser(login))
                    console.WriteLine("deleted user " + login);
                else
                    console.WriteLine("user not found");
            }
        }

        private SortedDictionary<string, Command> commands = new SortedDictionary<string, Command>();

        public CommandManager()
        {
            RegisterCommand(new CmdUserAdd());
            RegisterCommand(new CmdUserSt());
            RegisterCommand(new CmdUserDel());
        }

        private void RegisterCommand(Command cmd)
        { commands.Add(cmd.Name, cmd); }
        
        public void HandleCommand(string cmdLine, PipeConsole console)
        {
            // XXX: add users/change settings/etc
            var args = StringUtil.SplitCommandLine(cmdLine).ToArray();
            if (args.Length == 0)
            {
                // XXX: print help
                return;
            }
            Command cmd;
            if (!commands.TryGetValue(args[0], out cmd))
            {
                // XXX: print help
                return;
            }
            cmd.Run(args, console);
        }
    }
}
