using commandes;
using commandes.Interfaces;
using SimConnect.NET;
using System;
using System.Threading;

namespace ia
{
    public class GliderEnv
    {
        private SimConnectClient client;
        private Icommandes controls;
        private float lastAltitude;

        public GliderEnv(SimConnectClient client)
        {
            this.client = client;
            controls = new SimConnectControls(client);
        }

        public float[] Reset()
        {
            lastAltitude = GetAltitude();
            return GetState();
        }

        public (float[] state, float reward, bool done) Step(float[] action)
        {
            // Appliquer commandes
            controls.SetAileron(action[0]);
            controls.SetElevator(action[1]);
            controls.SetRudder(action[2]);

            Thread.Sleep(100); // attendre que les commandes prennent effet

            float altitude = GetAltitude();
            float reward = altitude - lastAltitude; // simple reward : gain en altitude
            lastAltitude = altitude;

            var state = GetState();
            bool done = altitude < 0 || altitude > 30000; // crash ou sortie de l'altitude

            return (state, reward, done);
        }

        private float GetAltitude()
        {
            return (float)client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet").Result;
        }

        private float[] GetState()
        {
            float alt = GetAltitude() / 30000f;
            float speed = (float)client.SimVars.GetAsync<double>("AIRSPEED INDICATED", "knots").Result / 200f;
            float bank = (float)client.SimVars.GetAsync<double>("PLANE BANK DEGREES", "radians").Result / (float)Math.PI;
            float pitch = (float)client.SimVars.GetAsync<double>("PLANE PITCH DEGREES", "radians").Result / (float)Math.PI;
            float vario = (GetAltitude() - lastAltitude) / 50f;

            return new float[] { alt, speed, bank, pitch, vario };
        }
    }
}
