using commandes.Interfaces;
using System.Drawing;

namespace autopilote
{
    internal static class Drive
    {
        static bool leftStartHeading = false;
        // Vol plat
        public static async Task vol_Plat(Icommandes controls, double angleRoulis)
        {
            double angle_max = 0.01;
            double angle = 0.05;

            if (angleRoulis > angle_max)
                await controls.SetAileron(angle);

            if (angleRoulis < -angle_max)
                await controls.SetAileron(-angle);

            if (Math.Abs(angleRoulis) <= angle_max)
                await controls.SetAileron(0);
        }
        // Toujour tenir le cap
        public static async Task cap(Icommandes controls, double boussole, double boussole_start)
        {
            double angle = 0.05;
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
        // Faire un cercle 
        public static async Task<bool> CircleLeft(Icommandes controls, double boussole, double boussole_start)
        {
            double angle = 0.05;

            // Virage constant
            await controls.SetAileron(-angle);
            await controls.SetRudder(-angle);

            await controls.SetElevator(0.02);

            double diff = NormalizeAngle(boussole - boussole_start);

            // Si la valeur absolue est plus grande que 20°, on a quitté le cap de départ
            if (Math.Abs(diff) > 80)
                leftStartHeading = true;
            // Si on a quitté le cap de départ et qu'on y est revenu, on arrête le cercle (on revient en vol en ligne droite)
            if (leftStartHeading && Math.Abs(diff) < 80)
            {
                await controls.SetAileron(0);
                await controls.SetRudder(0);
                await controls.SetElevator(0);
                leftStartHeading = false;
                Console.WriteLine("Cercle gauche terminé");
                return false;
            }
            return true;
        }
        static double NormalizeAngle(double angle)
        {
            angle %= 360;
            if (angle < -180) angle += 360;
            if (angle > 180) angle -= 360;
            return angle;
        }

        public static async Task<bool> thermique(Icommandes controls, double vario)
        {
            if (vario >= 6)
            {
                return true;
            }
            return false;
        }

        public static async Task tourner(Icommandes controls, double angleRoulis, double angleFinal = 40)
        {
            double Kp = 0.005; // sensibilité (proportionnel)

            while (Math.Abs(angleRoulis - angleFinal) > 1) // tolérance 1°
            {
                double erreur = angleFinal - angleRoulis;
                double aileronCmd = Math.Clamp(erreur * Kp, -0.1, 0.1);

                await controls.SetAileron(aileronCmd);

                await Task.Delay(50); // laisse le temps au roulis de changer
            }

            // stabilise une fois terminé
            await controls.SetAileron(0);
        }
    }
}