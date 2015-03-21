using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace counterDivider_Test
{
    public class Program
    {

        static InterruptPort counterRead;
        static InterruptPort pwmRead;
        static InterruptPort resetButton;

        static double counterCounter = 1;
        static double pwmCounter = 1;


        static void counterRead_OnInterrupt(uint port, uint state, DateTime time)
        {
            counterCounter++;
        }

        //static void pwmRead_OnInterrupt(uint port, uint state, DateTime time)
        //{
        //    pwmCounter++;
        //}

        static void resetButton_OnInterrupt(uint port, uint state, DateTime time)
        {
            
            pwmCounter=1;
            counterCounter=1;
        }


        public static void Main()
        {
            //Disable the garbage collector messages
            Debug.EnableGCMessages(false);

            counterRead =  new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.IO48, false,
                                   Port.ResistorMode.PullUp,
                                   Port.InterruptMode.InterruptEdgeLow);
            
            counterRead.OnInterrupt += new NativeEventHandler(counterRead_OnInterrupt);

            //pwmRead = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.IO47, true,
            //                       Port.ResistorMode.PullDown,
            //                       Port.InterruptMode.InterruptEdgeLow);

            //pwmRead.OnInterrupt += new NativeEventHandler(pwmRead_OnInterrupt);


            resetButton = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.LDR, true,
                                   Port.ResistorMode.PullUp,
                                   Port.InterruptMode.InterruptEdgeLow);
            resetButton.OnInterrupt += new NativeEventHandler(resetButton_OnInterrupt);

            PWM pwm = new PWM((PWM.Pin)FEZ_Pin.PWM.Di9);
            byte duty = 50;
            int freq = 500;

            //pwm.Set(false);
            //pwm.Set(freq, duty);

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, true);

            while (true)
            {
                //Debug.Print("Counter: " + counterCounter.ToString() + " PWM: " + pwmCounter.ToString() +
                //    " Ratio : " + ((counterCounter / pwmCounter) * 100).ToString());

                Debug.Print("Counter: " + counterCounter.ToString());

                Thread.Sleep(100);

               
            }
        }

    }
}
