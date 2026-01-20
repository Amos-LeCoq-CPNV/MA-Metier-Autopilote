using SimConnect.NET;
using System.IO;

var client = new SimConnectClient();
await client.ConnectAsync();

bool running = true;

// Instruction d'arrêt pour l'utilisateur

Console.WriteLine("Appuie sur Q pour arrêter l’enregistrement");

// Création du fichier de données

using var writer = new StreamWriter("FlightData.csv");
writer.WriteLine("Timestamp;Altitude_ft;Airspeed_kts;AccelX;AccelY;AccelZ");

// Boucle principale du fonctionnement du programme

while (running)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Q)
            running = false;
    }

    // Création des variable stockant les données à écrire

    var altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet");
    var airspeed = await client.SimVars.GetAsync<double>("AIRSPEED INDICATED", "knots");

    var ax = await client.SimVars.GetAsync<double>("ACCELERATION BODY X", "feet per second squared");
    var ay = await client.SimVars.GetAsync<double>("ACCELERATION BODY Y", "feet per second squared");
    var az = await client.SimVars.GetAsync<double>("ACCELERATION BODY Z", "feet per second squared");

    // Ecriture des variables

    writer.WriteLine($"{DateTime.Now:HH:mm:ss.fff};{altitude:F0};{airspeed:F0};{ax:F3};{ay:F3};{az:F3}");
    writer.Flush();

    // Rythme de la boucle et capture de données

    await Task.Delay(100);
}

// Déclaration que le programme est arrêté

Console.WriteLine("Enregistrement arrêté.");

await Task.Delay(100);