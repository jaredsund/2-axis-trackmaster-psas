using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;


namespace PWMNunchuk_Test
{
    public class Program
    {
        public static void Main()
        {
            //Disable the garbage collector messages
            Debug.EnableGCMessages(false);

            // This demo is for the Nunchuk controller. If you are using Classic controller then
            // replace FEZ_Components.Wii.Nunchuk with FEZ_Components.Wii.ClassicController
            FEZ_Components.Wii.Nunchuk.Initialize();
            while (true)
            {
                Debug.Print(
                    " Analog X: " + FEZ_Components.Wii.Nunchuk.AnalogStickX + 
                    " Analog Y: " + FEZ_Components.Wii.Nunchuk.AnalogStickY +
                    " Accel X: " + FEZ_Components.Wii.Nunchuk.AccelerateX + 
                    " Accel Y: " + FEZ_Components.Wii.Nunchuk.AccelerateY +
                    " Button C: " + FEZ_Components.Wii.Nunchuk.ButtonC.ToString () +
                    " Button Z: " + FEZ_Components.Wii.Nunchuk.ButtonZ.ToString ()
                    );
                Thread.Sleep(100);
            }
        }

    }
}
