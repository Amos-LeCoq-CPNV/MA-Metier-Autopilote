using ia;
using System;
using System.Threading.Tasks;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Démarrage de l'entraînement RL du planeur...");
        await Trainer.Run();
        Console.WriteLine("Entraînement terminé.");
        Console.ReadLine();
    }
}
