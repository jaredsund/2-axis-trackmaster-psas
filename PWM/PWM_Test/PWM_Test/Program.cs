using System;
using Microsoft.SPOT;
using System.Threading;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;



namespace PWM_Test
{
    public class Program
    {

        //digital signal for motor controler
        static OutputPort xEnable;
        static OutputPort xDir;

        static InterruptPort xMotorGo;
        static InterruptPort xMotorDir;

        static bool checkXEnable;
        static bool checkXDir;

        static OutputPort eCodePWR;
        static InterruptPort eCodeA;

        static long eCodeACount;

        static AnalogIn pot1Voltage;
        
        static PWM pwm;
        static PWM pwm2;

        static byte duty;
        static int freq;
        
        static void xMotorGo_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (checkXEnable == true)
            {
                pwm.Set(true);
                checkXEnable = false; 
            }
            else
            {
                eCodeACount = 0;
                pwm.Set(false);
                pwm.Set(freq, duty);
                checkXEnable = true;
                
            }//end if
        }

        static void xMotorDir_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (checkXDir == true)
            {
                pwm2.Set(true);
                //xDir.Write(false);
                checkXDir = false;
            }
            else
            {

                
                pwm2.Set(false);
                pwm2.Set(freq, duty);
                checkXDir = true;
            }//end if
        }

        static void eCodeA_OnInterrupt(uint port, uint state, DateTime time)
        {
            eCodeACount++;
            if (eCodeACount > 10000)
            {
                
                checkXEnable = false;
                pwm.Set(true);
                //xDir.Write(!checkXDir);
                //xDir.Write(checkXDir);
            }//end if

        }

        public static void Main()
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, 400);//400ms
            Microsoft.SPOT.Hardware.Cpu.GlitchFilterTime = ts;

            eCodePWR = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO54, true);
            eCodeA = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.IO49, false, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);
            eCodeA.OnInterrupt += new NativeEventHandler(eCodeA_OnInterrupt);

            eCodeACount = 0;

            pot1Voltage = new AnalogIn((AnalogIn.Pin)FEZ_Pin.AnalogIn.An3);
            pot1Voltage.SetLinearScale(0, 3300);
            int voltage1 = pot1Voltage.Read();

            checkXEnable = false;
            checkXDir = false;
           
            xEnable = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO45, false);
            xDir = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.IO46, false);

            xMotorGo = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.IO14, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh );
            xMotorDir = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.IO16, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh );

            xMotorGo.OnInterrupt += new NativeEventHandler(xMotorGo_OnInterrupt);
            xMotorDir.OnInterrupt += new NativeEventHandler(xMotorDir_OnInterrupt);

            duty = 50;
            freq = 100;

            pwm = new PWM((PWM.Pin)FEZ_Pin.PWM.Di10);

            pwm2 = new PWM((PWM.Pin)FEZ_Pin.PWM.Di9);

            int freqTemp = freq;
            xEnable.Write(true);
            xDir.Write(true);

            while (true)
            {
                freqTemp = freq;   
                Thread.Sleep(100);

                voltage1 = pot1Voltage.Read();
                freq = (voltage1/10*10);
                
                //if (freqTemp != freq)
                //{
                //    pwm.Set(freq, duty);
                //}//end if

                Debug.Print("Voltage1: " + voltage1.ToString());
                Debug.Print("Freq: " + freq.ToString());
                Debug.Print("EnCodeA Count: " + eCodeACount.ToString());

            }//end while 

        }

    }
}
