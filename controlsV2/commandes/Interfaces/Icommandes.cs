using System;
using System.Collections.Generic;
using System.Text;

namespace commandes.Interfaces
{
    public interface Icommandes
    {
        Task SetAileron(double value);
        Task SetElevator(double value);
        Task SetRudder(double value);
        Task SetAutopilotHeading(int heading);
    }
}