using SimConnect.NET;
using System.IO;

var client = new SimConnectClient();
await client.ConnectAsync();

bool running = true;

Console.WriteLine("Appuie sur Q pour arrêter l’enregistrement");

using var writer = new StreamWriter("FlightData.csv");
writer.WriteLine("Timestamp;Altitude_ft;Airspeed_kts;AccelX;AccelY;AccelZ");

while (running)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Q)
            running = false;
    }

    var altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet");
    var airspeed = await client.SimVars.GetAsync<double>("AIRSPEED INDICATED", "knots");

    var ax = await client.SimVars.GetAsync<double>("ACCELERATION BODY X", "feet per second squared");
    var ay = await client.SimVars.GetAsync<double>("ACCELERATION BODY Y", "feet per second squared");
    var az = await client.SimVars.GetAsync<double>("ACCELERATION BODY Z", "feet per second squared");

    writer.WriteLine($"{DateTime.Now:HH:mm:ss.fff};{altitude:F0};{airspeed:F0};{ax:F3};{ay:F3};{az:F3}");
    writer.Flush();

    await Task.Delay(100);
}

Console.WriteLine("Enregistrement arrêté.");
