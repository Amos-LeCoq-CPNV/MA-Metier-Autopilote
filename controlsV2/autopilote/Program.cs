using commandes.Interfaces;
using commandes;
using SimConnect.NET;
using System.Globalization;
using MySql.Data.MySqlClient;

class Program
{
    static async Task Main() { await connect(); }

    // Variables d'état

    // Indique si on est en train de faire un cercle
    static bool doCircle = false;

    // Indique si on s'est déjà éloigné du cap de départ
    static bool leftStartHeading = false;

    static async Task connect()
    {
        // Connexion MySQL
        string connectionString = "Server=localhost;Port=3306;Database=mydb;User Id=root;Password=Pa$$w0rd;";

        // Enregistrement CSV
        string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "MA-Metier-Autopilote");

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
        Console.WriteLine("Q = quitter | C = cercle gauche");

        using var writer = new StreamWriter(csvPath);
        writer.WriteLine("Timestamp;Altitude_ft;Airspeed_kts;AccelX;AccelY;AccelZ");

        bool boussoleInfo = true;
        double boussole_start = 0;

        while (running)
        {
            // Gestion clavier
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Q)
                    running = false;

                if (key.Key == ConsoleKey.C)
                    doCircle = true;
            }

            // Données avion
            double altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet");
            double airspeed = await client.SimVars.GetAsync<double>("AIRSPEED INDICATED", "knots");
            double magnetic_compas = await client.SimVars.GetAsync<double>("MAGNETIC COMPASS", "degrees");
            double roulis = await client.SimVars.GetAsync<double>("PLANE BANK DEGREES", "radians");

            double ax = await client.SimVars.GetAsync<double>("ACCELERATION BODY X", "feet per second squared");
            double ay = await client.SimVars.GetAsync<double>("ACCELERATION BODY Y", "feet per second squared");
            double az = await client.SimVars.GetAsync<double>("ACCELERATION BODY Z", "feet per second squared");

            // Mémorisation du cap de départ (une seule fois)
            if (boussoleInfo)
            {
                boussole_start = magnetic_compas;
                boussoleInfo = false;
            }

            // Analyse / pilotage
            await analyse(client, altitude, airspeed, magnetic_compas, roulis, boussole_start);

            // CSV
            DateTime timestamp = DateTime.Now;
            writer.WriteLine(
                $"{timestamp:HH:mm:ss.fff};" +
                $"{altitude.ToString("F0", CultureInfo.InvariantCulture)};" +
                $"{airspeed.ToString("F0", CultureInfo.InvariantCulture)};" +
                $"{ax.ToString("F3", CultureInfo.InvariantCulture)};" +
                $"{ay.ToString("F3", CultureInfo.InvariantCulture)};" +
                $"{az.ToString("F3", CultureInfo.InvariantCulture)}"
            );
            writer.Flush();

            // SQL
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

    // Analyse / pilotage
    public static async Task analyse(SimConnectClient client,double alt,double vitesse,double boussole,double angleRoulis,double boussole_start)
    {
        Icommandes controls = new SimConnectControls(client);

        // Si on fait un cercle
        if (doCircle)
        {
            await CircleLeft(controls, boussole, boussole_start);
            return;
        }

        // Vol en ligne droite
        double angle_max = 0.01;
        double angle = 0.05;

        if (angleRoulis > angle_max)
            await controls.SetAileron(angle);

        if (angleRoulis < -angle_max)
            await controls.SetAileron(-angle);

        if (Math.Abs(angleRoulis) <= angle_max)
            await controls.SetAileron(0);

        // Boussole
        Console.Write(boussole_start + " " + boussole + " " + angleRoulis + "\n");
        double boussole_erreur = NormalizeAngle(boussole - boussole_start);
        double boussole_tolerance = 2;

        // trop à droite
        if (boussole_erreur > boussole_tolerance)
        {
            await controls.SetRudder(-angle);
        }

        // trop à gauche
        if (boussole_erreur < -boussole_tolerance)
        {
            await controls.SetRudder(angle);
        }

        // ok
        if (Math.Abs(boussole_erreur) <= boussole_tolerance)
        {
            await controls.SetRudder(0);
        }
    }

    // Cercle complet (gauche)
    static async Task CircleLeft(Icommandes controls, double boussole, double boussole_start)
    {
        double angle = 0.05;

        // Initialisation du cercle

        // Virage constant
        await controls.SetAileron(-angle);
        await controls.SetRudder(-angle);

        double diff = NormalizeAngle(boussole - boussole_start);

        // Si la valeur absolue est plus grande que 20°, on a quitté le cap de départ
        if (Math.Abs(diff) > 20)
            leftStartHeading = true;

        // Si on a quitté le cap de départ et qu'on y est revenu, on arrête le cercle (on revient en vol en ligne droite)
        if (leftStartHeading && Math.Abs(diff) < 2)
        {
            await controls.SetAileron(0);
            await controls.SetRudder(0);

            doCircle = false;

            Console.WriteLine("Cercle gauche terminé");
        }
    }

    // Outil angles
    static double NormalizeAngle(double angle)
    {
        angle %= 360;
        if (angle < -180) angle += 360;
        if (angle > 180) angle -= 360;
        return angle;
    }
}
