using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sdm.Core;

namespace Sdm.Server
{
    internal sealed class UserList
    {
        /* userlist entry format:           
           [user_0]
             login = john.doe
             password = NmU0ZTMzZjc0NDE5N2ZlZmE4Yjc2Y2U3NDJmOGUyM2M=
             access = 3           
         */
        private readonly object sync = 1;
        private SortedList<string, UserAccount> users;
        public const string FileName = "sdm_server_users.ini";

        public UserList()
        { users = new SortedList<string, UserAccount>(); }

        public int Count { get { return users.Count; } }

        public UserAccount FindUser(string login)
        {
            lock (sync)
            {
                UserAccount acc;
                return users.TryGetValue(login, out acc) ? acc : null;
            }
        }

        public bool AddUser(UserAccount acc)
        {
            lock (sync)
            {
                if (users.ContainsKey(acc.Login))
                    return false;
                users.Add(acc.Login, acc);
                return true;
            }
        }

        public bool RemoveUser(string name)
        {
            lock (sync)
            {
                if (!users.ContainsKey(name))
                    return false;
                users.Remove(name);
                return true;
            }
        }

        public void Load()
        {
            if (!File.Exists(FileName))
                return;
            lock (sync)
            {
                var cfg = new IniFile(FileName, Encoding.UTF8);
                var userCount = cfg.GetSectionCount();
                users.Capacity = userCount;
                for (int i = 0; i < userCount; i++)
                {
                    var sect = "user_" + i;
                    // XXX: handle errors
                    var login = cfg.GetString(sect, "login");
                    var password = cfg.GetString(sect, "password");
                    var access = (ClientAccessFlags) cfg.GetInt32(sect, "access");
                    var acc = new UserAccount(login, password, access);
                    users.Add(login, acc);
                }
            }
        }

        public void Save()
        {
            using (var w = new StreamWriter(FileName, false, Encoding.UTF8))
            {
                int i = 0;
                lock (sync)
                {
                    foreach (var kv in users)
                    {
                        var acc = kv.Value;
                        w.Write("[user_");
                        w.Write(i);
                        w.WriteLine(']');
                        w.Write("login = ");
                        w.WriteLine(acc.Login);
                        w.Write("password = ");
                        w.WriteLine(acc.Password);
                        w.Write("access = ");
                        w.WriteLine((int)acc.Access);
                        i++;
                    }
                }
            }
        }
    }
}
