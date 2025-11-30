using PlainFiles.Core.Models;

namespace PlainFiles.Core
{
    public class NugetCsvHelper
    {
        public List<Person> Read(string path)
        {
            var people = new List<Person>();

            foreach (var line in File.ReadLines(path).Skip(1)) 
            {
                var parts = line.Split(',');

                people.Add(new Person
                {
                    Id = int.Parse(parts[0]),
                    FirstName = parts[1],
                    LastName = parts[2],
                    Phone = parts[3],
                    City = parts[4],
                    Balance = decimal.Parse(parts[5])
                });
            }

            return people;
        }

        public void Write(string path, List<Person> records)
        {
            using var writer = new StreamWriter(path);

            writer.WriteLine("Id,FirstName,LastName,Phone,City,Balance");

            foreach (var p in records)
            {
                writer.WriteLine($"{p.Id},{p.FirstName},{p.LastName},{p.Phone},{p.City},{p.Balance}");
            }
        }
    }
}

