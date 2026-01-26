using commandes.Interfaces; // Icommandes
using controls.Wrapper;      // SimConnectControls
using SimConnect.NET;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using MySql.Data.MySqlClient;

class Program
{
    static async Task Main() { await connect(); }

    static async Task connect()
    {
        // Connexion MySQL
        string connectionString =
            "Server=localhost;Port=3306;Database=mydb;User Id=root;Password=Pa$$w0rd;";

        // Chemin du CSV (idéalement externe au projet)
        string csvPath = "FlightData.csv";

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

        bool bousoleInfo= true;
        double bousole_start = 0;

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

            double magnetic_compas = await client.SimVars.GetAsync<double>("MAGNETIC COMPASS", "degrees");
            double roulis = await client.SimVars.GetAsync<double>("PLANE BANK DEGREES", "radians");

            double ax = await client.SimVars.GetAsync<double>("ACCELERATION BODY X", "feet per second squared");
            double ay = await client.SimVars.GetAsync<double>("ACCELERATION BODY Y", "feet per second squared");
            double az = await client.SimVars.GetAsync<double>("ACCELERATION BODY Z", "feet per second squared");

            if (bousoleInfo == true)
            {
                bousole_start = magnetic_compas;
                bousoleInfo= false;
            }
            
            await analyse(client, altitude, airspeed, magnetic_compas, roulis, bousole_start);

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
    }

    public static async Task analyse(SimConnectClient client, double alt, double vitesse, double boussole, double angleRoulis,double boussole_start)
    {
        await Task.Delay(100);
        // Instanciation de la toolbox
        Icommandes controls = new SimConnectControls(client);
        Console.WriteLine(alt.ToString()+" "+vitesse.ToString()+" "+angleRoulis.ToString());

        double angle_max = 0.01;
        if (angleRoulis>angle_max)
        {
            await controls.SetAileron(0.05);
        }
        if (angleRoulis < -angle_max)
        {
            await controls.SetAileron(-0.05);
        }
        if ((angleRoulis < angle_max) && (angleRoulis > -angle_max))
        {
            await controls.SetAileron(0);
        }

        /*
        if (vitesse> N)
            {
                await controls.SetElevator(0.2);
            }
            if (vitesse< N - 2)
            {
                await controls.SetElevator(-0.2);
            }
            if (vitesse==0)
            {
                await controls.SetElevator(0);
            }
        */
        Console.Write(boussole_start + " " + boussole+"\n");
        
        if (boussole > boussole_start+2)
        {
            await controls.SetRudder(-0.05);
        }
        if (boussole < boussole_start - 2)
        {
            await controls.SetRudder(0.05);
        }
        if ((boussole < boussole_start+2) && (boussole > boussole_start - 2))
        {
            await controls.SetRudder(0);
        }
    }
}
//await controls.SetAutopilotHeading(180);