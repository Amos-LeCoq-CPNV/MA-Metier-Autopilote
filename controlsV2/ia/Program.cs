using System;

namespace ia
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Démarrage de l'entraînement RL du planeur...");

            Trainer.Run();

            Console.WriteLine("Entraînement terminé.");
            Console.ReadLine();
        }
    }
}
