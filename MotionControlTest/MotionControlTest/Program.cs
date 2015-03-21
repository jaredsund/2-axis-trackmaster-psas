using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;



namespace MotionControlTest
{
    public class Program
    {
        const int freqMax = 2500;
  
        static InterruptPort resetButton;
        static bool systemOn = false;

        static MotorController mcY;
        static MotorController mcX;
      
        static void resetButton_OnInterrupt(uint port, uint state, DateTime time)
        {
            systemOn = !systemOn;
            mcY.Enable(systemOn);
            mcX.Enable(systemOn);
        }

        public static void Main()
        {
            //Disable the garbage collector messages
            Debug.EnableGCMessages(false);

            FEZ_Components.Wii.Nunchuk.Initialize();

            resetButton = new InterruptPort((Cpu.Pin)FEZ_Pin.Interrupt.LDR, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            resetButton.OnInterrupt += new NativeEventHandler(resetButton_OnInterrupt);

            LED_Indicators myIndicators = new LED_Indicators(FEZ_Pin.Digital.IO54, FEZ_Pin.Digital.IO55, FEZ_Pin.Digital.IO56);
            AntSelector myAntSel = new AntSelector(FEZ_Pin.Digital.IO61, FEZ_Pin.Digital.IO62, FEZ_Pin.Digital.IO63, FEZ_Pin.Digital.IO64);
            Antenna myAntenna = new Antenna(FEZ_Pin.AnalogIn.An1, FEZ_Pin.AnalogIn.An2, FEZ_Pin.AnalogIn.An3, FEZ_Pin.AnalogIn.An4);
           
            maxSpeedControl mSC = new maxSpeedControl(freqMax, FEZ_Pin.AnalogIn.An5, FEZ_Pin.Digital.IO13, FEZ_Pin.Digital.IO14, FEZ_Pin.Digital.IO16, FEZ_Pin.Digital.IO17);
            mcY = new MotorController("Y", FEZ_Pin.Digital.IO45, FEZ_Pin.Digital.IO46, FEZ_Pin.PWM.Di10, FEZ_Pin.Interrupt.IO48, FEZ_Pin.Digital.IO57, FEZ_Pin.Digital.IO58, freqMax, 250, false);
            mcX = new MotorController("X", FEZ_Pin.Digital.IO44, FEZ_Pin.Digital.IO47, FEZ_Pin.PWM.Di9, FEZ_Pin.Interrupt.IO49, FEZ_Pin.Digital.IO59, FEZ_Pin.Digital.IO60, freqMax, 100, true);

            int xThumbHome = 0;
            int yThumbHome = 0;
            int freqY, freqX;

            bool homeSet = false;

            
            bool running = false;
            int runFreq = mSC.freq;

            while (true)//main program loop
            {
                if (FEZ_Components.Wii.Nunchuk.ButtonC == true && FEZ_Components.Wii.Nunchuk.ButtonZ == true)
                {
                    mcY.home();
                    mcX.home();
                    Thread.Sleep(200);
                }
                else if (FEZ_Components.Wii.Nunchuk.ButtonC == true) //manual mode
                {
                    myIndicators.yellow();

                    if (homeSet == false)
                    {
                        xThumbHome = FEZ_Components.Wii.Nunchuk.AnalogStickX;
                        yThumbHome = FEZ_Components.Wii.Nunchuk.AnalogStickY;
                        mcY.freq = mSC.freq;
                        mcX.freq = mcY.freq;
                        homeSet = true;
                    }

                    freqY = System.Math.Abs((yThumbHome - FEZ_Components.Wii.Nunchuk.AnalogStickY) * 10);
                    freqX = System.Math.Abs((xThumbHome - FEZ_Components.Wii.Nunchuk.AnalogStickX) * 10);

                    if (freqY > freqX)
                    {
                        mcX.Stop();
                        mcY.Go(freqY, (FEZ_Components.Wii.Nunchuk.AnalogStickY < 0), false);
                    }
                    else
                    {
                        mcY.Stop();
                        mcX.Go(freqX, (FEZ_Components.Wii.Nunchuk.AnalogStickX < 0), false);
                    }//end if
                }
                else if (FEZ_Components.Wii.Nunchuk.ButtonZ == true) //Automated motion
                {
                    myIndicators.red();

                    if (!running)
                    {
                        runFreq = mSC.freq;
                        myAntenna.setBase();
                        mcY.Go(runFreq, myAntenna.Status.yDir, false, myAntenna.Status.deltaRatio);
                        mcX.Go(runFreq, myAntenna.Status.xDir, false, myAntenna.Status.deltaRatio);
                    }
                    else
                    {
                        mcY.Dir(myAntenna.Status.yDir, myAntenna.Status.deltaRatio);
                        mcX.Dir(myAntenna.Status.xDir, myAntenna.Status.deltaRatio);
                    }//end if

                    running = true;
                }
                else //no buttons pressed on the nunChuk
                {
                    myIndicators.green();
                    homeSet = false;
                    mcY.Stop();
                    mcX.Stop();
                    running = false;

                    Debug.Print(myAntenna.Status.message);

                    Thread.Sleep(200);
                }//end if

                //Thread.Sleep(50);
            }//end while
        }


    }

}

