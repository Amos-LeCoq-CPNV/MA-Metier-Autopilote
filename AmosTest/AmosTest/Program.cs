using SimConnect.NET;
using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

class Program
{
    static async Task Main()
    {
        // Connexion MySQL
        string connectionString =
            "Server=localhost;Port=3306;Database=mydb;User Id=root;Password=root;";

        // Chemin externe pour les données (AppData)
        string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MA-Metier-Autopilote"
        );

        Directory.CreateDirectory(basePath);

        string csvPath = Path.Combine(basePath, "FlightData.csv");

        using var sqlConnection = new MySqlConnection(connectionString);
        sqlConnection.Open();
        Console.WriteLine("Connexion MySQL réussie");

        // Connexion SimConnect
        var client = new SimConnectClient();
        await client.ConnectAsync();
        Console.WriteLine("Connexion à Flight Simulator réussie");

        bool running = true;
        Console.WriteLine("Appuie sur Q pour arrêter l’enregistrement");

        using var writer = new StreamWriter(csvPath);
        writer.WriteLine("Timestamp;Altitude_ft;Airspeed_kts;AccelX;AccelY;AccelZ");

        while (running)
        {
            // Arrêt avec Q
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Q)
                    running = false;
            }

            // Récupération des données
            double altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet");
            double airspeed = await client.SimVars.GetAsync<double>("AIRSPEED INDICATED", "knots");

            double ax = await client.SimVars.GetAsync<double>("ACCELERATION BODY X", "feet per second squared");
            double ay = await client.SimVars.GetAsync<double>("ACCELERATION BODY Y", "feet per second squared");
            double az = await client.SimVars.GetAsync<double>("ACCELERATION BODY Z", "feet per second squared");

            DateTime timestamp = DateTime.Now;

            // Écriture CSV (InvariantCulture = point comme séparateur décimal)
            writer.WriteLine(
                $"{timestamp:HH:mm:ss.fff};" +
                $"{altitude.ToString("F0", CultureInfo.InvariantCulture)};" +
                $"{airspeed.ToString("F0", CultureInfo.InvariantCulture)};" +
                $"{ax.ToString("F3", CultureInfo.InvariantCulture)};" +
                $"{ay.ToString("F3", CultureInfo.InvariantCulture)};" +
                $"{az.ToString("F3", CultureInfo.InvariantCulture)}"
            );
            writer.Flush();

            // Insertion SQL
            string query = @"
                INSERT INTO Data
                (`Timestamp`, `Altitude_ft`, `Airspeed_kts`, `AccelX_ft_s2`, `AccelY_ft_s2`, `AccelZ_ft_s2`)
                VALUES (@ts, @alt, @spd, @ax, @ay, @az)";

            using (var cmd = new MySqlCommand(query, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@ts", timestamp);
                cmd.Parameters.AddWithValue("@alt", (int)altitude);
                cmd.Parameters.AddWithValue("@spd", (int)airspeed);
                cmd.Parameters.AddWithValue("@ax", ax);
                cmd.Parameters.AddWithValue("@ay", ay);
                cmd.Parameters.AddWithValue("@az", az);

                cmd.ExecuteNonQuery();
            }

            await Task.Delay(100);
        }

        Console.WriteLine("Enregistrement arrêté.");
        Console.WriteLine($"Fichier CSV enregistré dans : {csvPath}");
    }
}
