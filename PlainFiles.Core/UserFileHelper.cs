using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlainFiles.Core.Models;
using System.Text;

namespace PlainFiles.Core
{
    public class UserFileHelper
    {
        public List<User> ReadUsers(string path)
        {
            var result = new List<User>();

            if (!File.Exists(path))
                return result;

            var lines = File.ReadAllLines(path, Encoding.UTF8);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts.Length >= 3)
                {
                    var username = parts[0].Trim();
                    var password = parts[1].Trim();
                    if (!bool.TryParse(parts[2].Trim(), out bool active))
                        active = false;

                    result.Add(new User
                    {
                        Username = username,
                        Password = password,
                        Active = active
                    });
                }
            }

            return result;
        }
        public void WriteUsers(string path, List<User> users)
        {
            var lines = users.Select(u => $"{u.Username},{u.Password},{u.Active}");
            File.WriteAllLines(path, lines, Encoding.UTF8);
        }
    }
}