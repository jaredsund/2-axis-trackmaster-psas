using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace MotionControlTest
{
    class maxSpeedControl
    {
        private AnalogIn potIN;
        private OutputPort LED1;
        private OutputPort LED2;
        private OutputPort LED3;
        private OutputPort LED4;

        private const int linearScaleMin = 0;
        private const int linearScaleMax = 3300;

        private int maxFreq;
    

        public maxSpeedControl(int maxFreq,FEZ_Pin.AnalogIn potINPUT, FEZ_Pin.Digital LED1, FEZ_Pin.Digital LED2, FEZ_Pin.Digital LED3, FEZ_Pin.Digital LED4 )
        {
            this.maxFreq = maxFreq;

            potIN = new AnalogIn((AnalogIn.Pin)potINPUT);
            potIN.SetLinearScale(linearScaleMin, linearScaleMax);

            this.LED1 = new OutputPort((Cpu.Pin)LED1, false);
            this.LED2 = new OutputPort((Cpu.Pin)LED2, false);
            this.LED3 = new OutputPort((Cpu.Pin)LED3, false);
            this.LED4 = new OutputPort((Cpu.Pin)LED4, false);

            //double j = msFactor; //take initial reading
            Thread speedControlThread;
            speedControlThread = new Thread(maxSpeedControlerThread);
            speedControlThread.Start();
        }

        private void maxSpeedControlerThread()
        {
            int t = 0;
            while (true)
            {
                t = freq; //poll the reading to update leds
                Thread.Sleep(100);
            }
        }

        public int voltage
        {
            get { return potIN.Read(); }
        }

        public int freq
        {
            get {return Convert.ToInt32 (System.Math.Floor (Convert.ToDouble( maxFreq.ToString ()) * msFactor).ToString ()) ; }
        }

        

        private void setDisplay(ref float passedValue)
        {
            if (passedValue > 0.75)
            {
                writeDisplay(true, true, true, true);   
            }
            else if (passedValue > 0.5)
            {
                writeDisplay(true, true, true, false);
            }
            else if (passedValue > 0.25)
            {
                writeDisplay(true, true, false, false);
            }
            else if (passedValue > 0)
            {
                writeDisplay(true, false, false, false);
            }
            else
            {
                writeDisplay(false, false, false, false);
            }//end if
        }

        private void writeDisplay(bool led1, bool led2, bool led3, bool led4)
        {
            LED1.Write(led1);
            LED2.Write(led2);
            LED3.Write(led3);
            LED4.Write(led4);
        }

        public double msFactor
        {
            get
            {
                float potAdjusted = (potIN.Read() / 10) * 10;
                float potRatio = potAdjusted / linearScaleMax;
                setDisplay(ref potRatio);
                return potRatio; 
            }//end get
        }
    }
}
