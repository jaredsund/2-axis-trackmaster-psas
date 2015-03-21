using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;


using GHIElectronics.NETMF.Hardware;

namespace PhotoCell_Test
{
    public class Program
    {
        public static void Main()
        {
            //Disable the garbage collector messages
            Debug.EnableGCMessages(false);

            AnalogIn photoCell = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An1);
            photoCell.SetLinearScale(0, 3300);

            while (true)
            {
                Debug.Print("PhotoCell Voltage: " + photoCell.Read().ToString());
            }
        }

    }
}
