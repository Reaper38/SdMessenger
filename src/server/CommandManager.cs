using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdm.Core;
using Sdm.Core.Util;

namespace Sdm.Server
{
    internal abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Format { get; }
        public abstract string Info { get; }
        
        public void PrintUsage(PipeConsole console)
        { console.WriteLine("usage: {0} {1} {2}", Root.AppName, Name, Format); }

        public abstract void Run(string[] args, PipeConsole console);
    }
    
    internal sealed class CommandManager
    {
        private sealed class CmdHelp : Command
        {
            public override string Name { get { return "help"; } }
            public override string Format { get { return "[command]"; } }
            public override string Info { get { return "print help"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (args.Length == 1)
                {
                    console.WriteLine("usage: {0} <command> [<args>]\r\n", Root.AppName);
                    var i = Root.CmdMgr.GetEnumerator();
                    while (i.MoveNext())
                    {
                        var cmd = i.Current;
                        console.WriteLine("    {0,-16} {1}", cmd.Name, cmd.Info);
                    }
                }
                else
                {
                    var cmdName = args[1];
                    var cmd = Root.CmdMgr.GetCommand(cmdName);
                    if (cmd == null)
                    {
                        console.WriteLine("{0}: unknown command: '{1}'", Root.AppName, cmdName);
                        return;
                    }
                    console.WriteLine("{0}: {1}", cmd.Name, cmd.Info);
                    cmd.PrintUsage(console);
                }
            }
        }

        private sealed class CmdUserAdd : Command
        {
            public override string Name { get { return "user.add"; } }
            public override string Format { get { return "<login> <pass> <access>"; } }
            public override string Info { get { return "create user account"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (args.Length < 4)
                {
                    PrintUsage(console);
                    return;
                }
                var login = args[1];
                var pass = args[2];
                string errMsg;
                if (!NetUtil.ValidateLogin(ref login, out errMsg))
                {
                    console.WriteLine("{0}: invalid login.\r\n{1}", Root.AppName, errMsg);
                    return;
                }
                if (!NetUtil.ValidatePassword(ref pass, out errMsg))
                {
                    console.WriteLine("{0}: invalid password.\r\n{1}", Root.AppName, errMsg);
                    return;
                }
                ClientAccessFlags access;
                if (!ClientAccessFlagsUtil.FromShortString(out access, args[3]))
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
                if (Root.Server.Users.Add(acc))
                    console.WriteLine("created user " + login);
                else
                    console.WriteLine("user already exists");
            }
        }

        private sealed class CmdUserSt : Command
        {
            public override string Name { get { return "user.st"; } }
            public override string Format { get { return "[login]"; } }
            public override string Info { get { return "show user account info"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (args.Length == 1)
                {
                    console.WriteLine("user count: " + Root.Server.Users.Count);
                    return;
                }
                if (args.Length >= 2)
                {
                    var users = Root.Server.Users;
                    var login = args[1];
                    var user = users.Find(login);
                    if (user == null)
                    {
                        console.WriteLine("user not found");
                        return;
                    }
                    console.WriteLine("user {0} : {1}", user.Login, ClientAccessFlagsUtil.ToShortString(user.Access));
                }
            }
        }

        private sealed class CmdUserDel : Command
        {
            public override string Name { get { return "user.del"; } }
            public override string Format { get { return "{<login>|--all}"; } }
            public override string Info { get { return "delete user account"; } }

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
                    users.Clear();
                    return;
                }
                if (users.Remove(login))
                    console.WriteLine("deleted user " + login);
                else
                    console.WriteLine("user not found");
            }
        }

        private sealed class CmdUserLoad : Command
        {
            public override string Name { get { return "user.load"; } }
            public override string Format { get { return ""; } }
            public override string Info { get { return "load user account list"; } }

            public override void Run(string[] args, PipeConsole console)
            { Root.Server.Users.Load(); }
        }

        private sealed class CmdUserSave : Command
        {
            public override string Name { get { return "user.save"; } }
            public override string Format { get { return ""; } }
            public override string Info { get { return "save current user account list"; } }

            public override void Run(string[] args, PipeConsole console)
            { Root.Server.Users.Save(); }
        }

        private sealed class CmdStop : Command
        {
            public override string Name { get { return "stop"; } }
            public override string Format { get { return ""; } }
            public override string Info { get { return "disconnect and stop server"; } }

            public override void Run(string[] args, PipeConsole console)
            {
                if (!Root.Server.Connected)
                    console.WriteLine("already disconnected");
                Root.Server.Disconnect();
            }
        }

        private readonly SortedDictionary<string, Command> commands = new SortedDictionary<string, Command>();
        private readonly Command cmdHelp;
        private readonly string[] emptyHelpArgs;

        public CommandManager()
        {
            cmdHelp = new CmdHelp();
            emptyHelpArgs = new[] {cmdHelp.Name};
            RegisterCommand(cmdHelp);
            RegisterCommand(new CmdUserAdd());
            RegisterCommand(new CmdUserSt());
            RegisterCommand(new CmdUserDel());
            RegisterCommand(new CmdUserLoad());
            RegisterCommand(new CmdUserSave());
            RegisterCommand(new CmdStop());
        }

        private void PrintHelp(PipeConsole console)
        { cmdHelp.Run(emptyHelpArgs, console); }

        private void RegisterCommand(Command cmd)
        { commands.Add(cmd.Name, cmd); }
        
        public Command GetCommand(string name)
        {
            Command cmd = null;
            commands.TryGetValue(name, out cmd);
            return cmd;
        }

        public IEnumerator<Command> GetEnumerator()
        { return commands.Values.GetEnumerator(); }

        public void HandleCommand(string cmdLine, PipeConsole console)
        {
            var args = StringUtil.SplitCommandLine(cmdLine).ToArray();
            if (args.Length == 0)
            {
                PrintHelp(console);
                return;
            }
            var cmd = GetCommand(args[0]);
            if (cmd == null)
            {
                PrintHelp(console);
                return;
            }
            cmd.Run(args, console);
        }
    }
}
