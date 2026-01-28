using commandes.Interfaces;

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
        public static async Task cap(Icommandes controls, double angleRoulis)
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
        // Faire un cercle 
        public static async Task<bool> CircleLeft(Icommandes controls, double boussole, double boussole_start)
        {
            double angle = 0.05;

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
    }
}