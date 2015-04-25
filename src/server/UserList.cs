using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sdm.Core;

namespace Sdm.Server
{
    internal sealed class UserList
    {
        private SortedList<string, UserAccount> users;

        public UserList()
        { users = new SortedList<string, UserAccount>(); }

        public UserAccount FindUser(string login)
        {
            UserAccount acc;
            return users.TryGetValue(login, out acc) ? acc : null;
        }

        public bool AddUser(UserAccount acc)
        {
            if (users.ContainsKey(acc.Login))
                return false;
            users.Add(acc.Login, acc);
            return true;
        }

        public bool RemoveUser(string name)
        {
            if (!users.ContainsKey(name))
                return false;
            users.Remove(name);
            return true;
        }

        public void Load(string filename)
        {
            if (!File.Exists(filename))
                return;
            var cfg = new IniFile(filename, Encoding.UTF8);
            var userCount = cfg.GetSectionCount();
            users.Capacity = userCount;
            for (int i = 0; i < userCount; i++)
            {
                var sect = "user_" + i;
                // XXX: handle errors
                var login = cfg.GetString(sect, "login");
                var password = cfg.GetString(sect, "password");
                var access = (ClientAccessFlags)cfg.GetInt32(sect, "access");
                var acc = new UserAccount(login, password, access);
                users.Add(login, acc);
            }
        }

        public void Save(string filename)
        {
            using (var w = new StreamWriter(filename, false, Encoding.UTF8))
            {
                int i = 0;
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
