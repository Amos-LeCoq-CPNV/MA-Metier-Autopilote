using commandes.Interfaces;
using SimConnect.NET;
using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;

namespace controls.Wrapper
{
    public class SimConnectControls : Icommandes
    {
        private readonly SimConnectClient _sim;

        // Constructeur
        public SimConnectControls(SimConnectClient sim)
        {
            _sim = sim ?? throw new ArgumentNullException(nameof(sim));
        }

        // Attention : toutes les méthodes async doivent être awaited
        public async Task SetAileron(double value)
        {
            value = Math.Max(-1.0, Math.Min(1.0, value));
            await _sim.SimVars.SetAsync("AILERON POSITION", "position", value);
        }
        public async Task SetElevator(double value)
        {
            value = Math.Max(-1.0, Math.Min(1.0, value));
            await _sim.SimVars.SetAsync("ELEVATOR POSITION", "position", value);
        }
        public async Task SetRudder(double value)
        {
            value = Math.Max(-1.0, Math.Min(1.0, value));
            await _sim.SimVars.SetAsync("RUDDER POSITION", "position", value);
        }

        public async Task SetAutopilotHeading(int heading)
        {
            heading = ((heading % 360) + 360) % 360; // normalize 0-359
            await _sim.SimVars.SetAsync("AUTOPILOT HEADING LOCK DIR", "degrees", heading);
        }
    }
}
