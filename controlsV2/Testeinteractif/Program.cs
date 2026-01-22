using commandes.Interfaces; // Icommandes
using controls.Wrapper;      // SimConnectControls
using SimConnect.NET;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Test interactif de la boîte à outils Controls ===");

        Console.WriteLine("Assurez-vous que MSFS est lancé et chargé...");
        await Task.Delay(5000);

        // Création et connexion au client SimConnect
        var client = new SimConnectClient("Test Controls");

        try
        {
            await client.ConnectAsync();
            Console.WriteLine("Connexion SimConnect OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur de connexion SimConnect : {ex.Message}");
            return;
        }

        // Instanciation de la toolbox
        Icommandes controls = new SimConnectControls(client);

        bool quitter = false;

        while (!quitter)
        {
            Console.WriteLine("\n--- Menu de test ---");
            Console.WriteLine("1 - Throttle à 75%");
            Console.WriteLine("2 - Aileron à 0.3");
            Console.WriteLine("3 - Elevator à -0.2");
            Console.WriteLine("4 - Rudder à 0.5");
            Console.WriteLine("5 - Flaps à 2");
            Console.WriteLine("6 - Autopilot Heading à 180°");
            Console.WriteLine("Q - Quitter");
            Console.Write("Choix : ");

            string choix = Console.ReadLine()?.Trim().ToUpper();

            try
            {
                switch (choix)
                {
                    case "1":
                        await controls.SetAileron(0.3);
                        Console.WriteLine("Aileron envoyé !");
                        break;
                    case "2":
                        await controls.SetElevator(-0.2);
                        Console.WriteLine("Elevator envoyé !");
                        break;
                    case "3":
                        await controls.SetRudder(0.5);
                        Console.WriteLine("Rudder envoyé !");
                        break;
                    case "4":
                        await controls.SetAutopilotHeading(180);
                        Console.WriteLine("Autopilot Heading envoyé !");
                        break;
                    case "Q":
                        quitter = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution : {ex.Message}");
            }
        }

        // Déconnexion propre
        await client.DisconnectAsync();
        Console.WriteLine("Déconnexion SimConnect OK");
    }
}
