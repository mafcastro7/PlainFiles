using PlainFiles.Core;
using PlainFiles.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

string logFile = "log.txt";
using var log = new LogWriter(logFile);
string usersFile = "Users.txt";

if (!File.Exists(usersFile))
{
    var defaultUsers = new List<string>
    {
        "jzuluaga,P@ssw0rd123!,true",
        "mbedoya,S0yS3gur02025*,false"
    };

    File.WriteAllLines(usersFile, defaultUsers);
}

var userHelper = new UserFileHelper();
var users = userHelper.ReadUsers(usersFile);

Console.WriteLine("=== LOGIN ===");

int attempts = 0;
User? loginAttemptUser = null;
User? loggedUser = null;

while (attempts < 3 && loggedUser == null)
{
    Console.Write("Username: ");
    string username = Console.ReadLine() ?? "";

    Console.Write("Password: ");
    string password = Console.ReadLine() ?? "";

    loginAttemptUser = users.FirstOrDefault(u=> u.Username == username);

    if (loginAttemptUser == null)
    {
        Console.WriteLine("Usuario no existe.");
        attempts++;
        continue;
    }

    if (!loginAttemptUser.Active)
    {
        Console.WriteLine("El usuario está bloqueado.");
        log.WriteLog("WARN", $"Intento de acceso por usuario bloqueado: {loginAttemptUser.Username}");
        return;
    }

    if (loginAttemptUser.Password == password)
    {
        loggedUser = loginAttemptUser;       
        Console.WriteLine($"Bienvenido {loggedUser.Username}!");
        log.WriteLog("INFO", $"El usuario '{loggedUser.Username}' inició sesión.");
    }
    else
    {
        Console.WriteLine("Contraseña incorrecta.");
        attempts++;
    }
}

if (loggedUser == null)
{
    if (loginAttemptUser != null)
    {
        loginAttemptUser.Active = false;
        userHelper.WriteUsers(usersFile, users);
        log.WriteLog("WARN", $"Usuario bloqueado por intentos fallidos: {loginAttemptUser.Username}");
    }

    Console.WriteLine("Usuario bloqueado por fallar tres intentos.");
    return;
}

string fileName = "people.csv";

if (!File.Exists(fileName))
{
    var initialData = new List<string[]>
    {
        new[] { "Id", "FirstName", "LastName", "Phone", "City", "Balance" },
        new[] { "1", "Maria", "Bedoya", "3223114015", "Medellín", "15000" },
        new[] { "2", "Juan", "Zuluaga", "3223114620", "Medellín", "8200" },
        new[] { "3", "Brad", "Pit", "3224504545", "Miami", "14000000" },
        new[] { "4", "xxx", "xx", "xxx", "xxxx", "5000" }
    };

    var manualCsv = new ManualCsvHelper();
    manualCsv.WriteCsv(fileName, initialData);

    log.WriteLog("INFO", $"'{loggedUser.Username}' creó automáticamente people.csv");
}

var helper = new NugetCsvHelper();
var people = helper.Read(fileName).ToList();

log.WriteLog("INFO", $"'{loggedUser.Username}' cargó {people.Count} registros de people.csv");

int option = -1;

while (option != 0)
{
    Console.WriteLine("===============================================");
    Console.WriteLine("1. Mostrar contenido.");
    Console.WriteLine("2. Añadir personas.");
    Console.WriteLine("3. Guardar cambios.");
    Console.WriteLine("4. Editar personas.");
    Console.WriteLine("5. Eliminar personas.");
    Console.WriteLine("6. Reporte por ciudad.");
    Console.WriteLine("0. Exit");
    Console.Write("Escoge una opción: ");

    int.TryParse(Console.ReadLine(), out option);

    Console.WriteLine();
    Console.WriteLine("===============================================");

    switch (option)
    {
        case 1:
            ShowContent(people);
            log.WriteLog("INFO", $"'{loggedUser.Username}' visualizó la lista de personas");
            break;

        case 2:
            AddPerson(people, loggedUser, log);
            break;

        case 3:
            helper.Write(fileName, people);
            Console.WriteLine("Cambios guardados!");
            log.WriteLog("INFO", $"'{loggedUser.Username}' guardó cambios en people.csv");
            break;

        case 4:
            EditPerson(people, loggedUser, log);
            break;

        case 5:
            DeletePerson(people, loggedUser, log);
            break;

        case 6:
            ReportByCity(people);
            break;
    }

    Console.WriteLine("===============================================");
    Console.WriteLine();
}

static void ShowContent(List<Person> people)
{
    foreach (var p in people)
    {
        Console.WriteLine($"{p.Id,-6}{p.FirstName} {p.LastName}");

        string indent = new string(' ', 6);

        Console.WriteLine($"{indent}Phone: {p.Phone}");
        Console.WriteLine($"{indent}City: {p.City}");

        string balance = $"${p.Balance:N2}".PadLeft(15);
        Console.WriteLine($"{indent}Balance:{balance}");

        Console.WriteLine();
    }
}

static void AddPerson(List<Person> people, User loggedUser, LogWriter log)
{
    int id;
    while (true)
    {
        Console.Write("ID:");
        string? idInput = Console.ReadLine();

        if (int.TryParse(idInput, out id) && id > 0)
        {
            bool idExists = people.Any(p => p.Id == id);

            if (!idExists)
                break;
            else
                Console.WriteLine("El ID ya existe. Ingrese un ID diferente.");
        }
        else
        {
            Console.WriteLine("Debe ingresar un número válido.");
        }
    }

    string first;
    while (true)
    {
        Console.Write("First name: ");
        first = Console.ReadLine()?.Trim() ?? "";
        bool onlyLetter = first.All(c => char.IsLetter(c) || c == ' ');

        if (first.Length > 0 && onlyLetter)
            break;

        Console.WriteLine("Debe ingresar un nombre válido.");
    }


    string last;
    while (true)
    {
        Console.Write("Last name: ");
        last = Console.ReadLine()?.Trim() ?? "";
        bool onlyLetter = last.All(c => char.IsLetter(c) || c == ' ');
        if (last.Length > 0 && onlyLetter)
            break;
        Console.WriteLine("Debe ingresar un apellido válido.");
    }

    string phone;
    while (true)
    {
        Console.Write("Phone: ");
        phone = Console.ReadLine()?.Trim() ?? "";
        bool isDigits = phone.All(char.IsDigit);

        if (phone.Length == 10 && isDigits)
            break;

        Console.WriteLine("El teléfono debe tener 10 dígitos.");
    }

    string city;
    while (true)
    {
        Console.Write("City: ");
        city = Console.ReadLine()?.Trim() ?? "";
        bool onlyLetter = city.All(c => char.IsLetter(c) || c == ' ');
        if (city.Length > 0 && onlyLetter)
            break;
        Console.WriteLine("Debe ingresar una ciudad.");
    }

    decimal balance;
    while (true)
    {
        Console.Write("Balance: ");
        string? balanceInput = Console.ReadLine();
        if (decimal.TryParse(balanceInput, out balance) && balance > 0)
            break;
        Console.WriteLine("Debe ingresar un número positivo.");
    }

    people.Add(new Person
    {
        Id = id,
        FirstName = first,
        LastName = last,
        Phone = phone,
        City = city,
        Balance = balance
    });

    Console.WriteLine("Persona añadida!");
    log.WriteLog("INFO", $"'{loggedUser.Username}' agregó a '{first} {last}' con ID {id}");
}

static void EditPerson(List<Person> people, User loggedUser, LogWriter log)
{
    Console.WriteLine("=== Editar Persona ===");
    Console.Write("Ingresa el ID a editar: ");
    string? inputId = Console.ReadLine();

    if (!int.TryParse(inputId, out int id))
    {
        Console.WriteLine("Inválido ID.");
        return;
    }

    var person = people.FirstOrDefault(p => p.Id == id);

    if (person == null)
    {
        Console.WriteLine("No se encontró persona con ese ID.");
        return;
    }

    Console.WriteLine("\nEditar a:");
    Console.WriteLine($"{person.Id}  {person.FirstName} {person.LastName}");
    Console.WriteLine($"Phone: {person.Phone}");
    Console.WriteLine($"City: {person.City}");
    Console.WriteLine($"Balance: {person.Balance:C}\n");
    Console.Write($"First name ({person.FirstName}): ");
    string newFirst = Console.ReadLine()?.Trim() ?? "";

    if (newFirst == "")
    {
        newFirst = person.FirstName;
    }
    else
    {
        bool onlyLetters = newFirst.All(c => char.IsLetter(c) || c == ' ');
        if (!onlyLetters)
        {
            Console.WriteLine("Caracteres inválidos. Se mantiene el valor anterior.");
            newFirst = person.FirstName;
        }
    }

    Console.Write($"Last name ({person.LastName}): ");
    string newLast = Console.ReadLine()?.Trim() ?? "";

    if (newLast == "")
    {
        newLast = person.LastName;
    }
    else
    {
        bool onlyLetters = newLast.All(c => char.IsLetter(c) || c == ' ');
        if (!onlyLetters)
        {
            Console.WriteLine("Caracteres inválidos. Se mantiene el valor anterior.");
            newLast = person.LastName;
        }
    }

    Console.Write($"Phone ({person.Phone}): ");
    string newPhone = Console.ReadLine()?.Trim() ?? "";

    if (newPhone == "")
    {
        newPhone = person.Phone;
    }
    else
    {
        bool isDigits = newPhone.All(char.IsDigit);

        if (!(isDigits && newPhone.Length == 10)) 
        {
            Console.WriteLine("Formato de teléfono inválido (debe ser 10 dígitos numéricos). Se mantiene el valor anterior.");
            newPhone = person.Phone;
        }
    }

    Console.Write($"City ({person.City}): ");
    string newCity = Console.ReadLine()?.Trim() ?? "";

    if (newCity == "")
    {
        newCity = person.City;
    }
    else
    {
        bool onlyLetters = newCity.All(c => char.IsLetter(c) || c == ' ');
        if (!onlyLetters)
        {
            Console.WriteLine("Caracteres inválidos. Se mantiene el valor anterior.");
            newCity = person.City;
        }
    }

    Console.Write($"Balance ({person.Balance}): ");
    string newBalanceInput = Console.ReadLine()?.Trim() ?? "";

    decimal newBalance;
    if (newBalanceInput == "")
    {
        newBalance = person.Balance;
    }
    else if (!decimal.TryParse(newBalanceInput, out newBalance) || newBalance <= 0)
    {
        Console.WriteLine("Balance inválido (debe ser un número mayor a cero). Se mantiene el valor anterior.");
        newBalance = person.Balance;
    }

    person.FirstName = newFirst;
    person.LastName = newLast;
    person.Phone = newPhone;
    person.City = newCity;
    person.Balance = newBalance;

    Console.WriteLine("\nPersona actualizada correctamente!");
    log.WriteLog("INFO", $"'{loggedUser.Username}' editó la persona con ID {person.Id}");
}


static void DeletePerson(List<Person> people, User loggedUser, LogWriter log)
{
    Console.WriteLine("=== Eliminar persona ===");
    Console.Write("Ingrese el ID de la persona a eliminar: ");
    string? inputId = Console.ReadLine();

    if (!int.TryParse(inputId, out int id))
    {
        Console.WriteLine("ID inválido.");
        return;
    }

    var person = people.FirstOrDefault(p => p.Id == id);

    if (person == null)
    {
        Console.WriteLine("No se encontró persona con ese ID.");
        return;
    }

    Console.WriteLine("\nPersona a borrar:");
    Console.WriteLine($"{person.Id}  {person.FirstName} {person.LastName}");
    Console.WriteLine($"Phone: {person.Phone}");
    Console.WriteLine($"City: {person.City}");
    Console.WriteLine($"Balance: {person.Balance:C}\n");

    Console.Write("¿Está seguro de eliminar a esta persona? (S/N):");
    string? confirm = Console.ReadLine()?.Trim().ToUpper();

    if (confirm != "S")
    {
        Console.WriteLine("Eliminación cancelada.");
        return;
    }

    people.Remove(person);

    Console.WriteLine("\n Persona eliminada satisfactoriamente.");

    log.WriteLog("INFO", $"'{loggedUser.Username}' eliminó a '{person.FirstName} {person.LastName}' con ID {person.Id}");
}

static void ReportByCity(List<Person> people)
{
    var grouped = people.GroupBy(p => p.City);
    decimal grandTotal = 0;
    foreach (var group in grouped)
    {
        Console.WriteLine($"\nCiudad: {group.Key}\n");
        Console.WriteLine($"{"ID",-6}{"Nombres",-15}{"Apellidos",-15}{"Saldo",15}");
        Console.WriteLine(new string('-', 51));

        decimal cityTotal = 0;

        foreach (var person in group)
        {
            Console.WriteLine($"{person.Id,-6}{person.FirstName,-15}{person.LastName,-15}{person.Balance,15:N2}");
            cityTotal += person.Balance;
        }
        Console.WriteLine(new string('-', 51));
        Console.WriteLine($"Total: {group.Key,-28}{cityTotal,15:N2}\n");

        grandTotal += cityTotal;
    }

    Console.WriteLine(new string('=', 51));
    Console.WriteLine($"Total General:{"",28}{grandTotal,15:N2}\n");
}