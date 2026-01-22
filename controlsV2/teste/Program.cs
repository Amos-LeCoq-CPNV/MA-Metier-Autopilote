using commandes.Interfaces; // Icommandes
using controls.Wrapper;      // SimConnectControls
using SimConnect.NET;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Test de la boîte à outils Controls ===");

        Console.WriteLine("Assurez-vous que MSFS est lancé et chargé...");
        await Task.Delay(5000); // 5 secondes d’attente

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
            return; // on arrête si pas de connexion
        }

        // Instanciation de la toolbox
        Icommandes controls = new SimConnectControls(client);

        // Test interactif des commandes
        await TestCommand("Aileron à 0.3", async () => await controls.SetAileron(0.3));
        await TestCommand("Elevator à -0.2", async () => await controls.SetElevator(-0.2));
        await TestCommand("Rudder à 0.5", async () => await controls.SetRudder(0.5));
        await TestCommand("Autopilot Heading à 180°", async () => await controls.SetAutopilotHeading(180));

        Console.WriteLine("=== Test terminé ===");

        // Déconnexion propre
        await client.DisconnectAsync();
        Console.WriteLine("Déconnexion SimConnect OK");

        Console.WriteLine("Appuyez sur ENTER pour quitter...");
        Console.ReadLine();
    }

    // Méthode helper pour interaction ENTER → action
    static async Task TestCommand(string description, Func<Task> action)
    {
        Console.WriteLine($"\nPress ENTER pour exécuter : {description}");
        Console.ReadLine();
        try
        {
            await action();
            Console.WriteLine($"{description} envoyé !");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de {description} : {ex.Message}");
        }
    }
}
