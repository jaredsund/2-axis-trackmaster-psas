using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace MotionControlTest
{
    
    class MotorController
    {
        private OutputPort enable;
        private OutputPort dir;
        private PWM pwm;
        private InputPort encoder;
        private OutputPort dirRed;
        private OutputPort dirgreen;

        private int encoderCounter;

        private const byte duty = 50;
        private int _freq;
        private int maxFreq;

        private int _home;
        private int maxEncoderLimit;
        private bool biDirectional;
        private int posFromHome;
        private bool maxDir1;
        private bool maxDir2;

        private string axis;

        public MotorController(string axis, FEZ_Pin.Digital enablePin, FEZ_Pin.Digital dirPin, FEZ_Pin.PWM pwmPin, FEZ_Pin.Interrupt encoderPin, FEZ_Pin.Digital dirLight1, FEZ_Pin.Digital dirLight2, int freq, int maxEncoderLimit, bool biDirectional)
        {
            this.maxEncoderLimit = maxEncoderLimit;
            this.biDirectional = biDirectional;
            maxDir1 = false;
            maxDir2 = false;

            this.freq = freq;
            maxFreq = freq;

            this.axis = axis;

            encoderCounter = 0;

            enable = new OutputPort((Cpu.Pin)enablePin, false);
            dir = new OutputPort((Cpu.Pin)dirPin, false);
            pwm = new PWM((PWM.Pin)pwmPin);

            encoder = new InterruptPort((Cpu.Pin)encoderPin, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);

            //encoder.OnInterrupt += biDirectional ?
            //   new NativeEventHandler(counterRead_OnInterrupt_biDirectional)
            //   : new NativeEventHandler(counterRead_OnInterrupt);

            encoder.OnInterrupt += new NativeEventHandler(counterRead_OnInterrupt);

            EnableInterrupt(false);

            dirRed = new OutputPort((Cpu.Pin)dirLight1, false);
            dirgreen = new OutputPort((Cpu.Pin)dirLight2, false);

            enable.Write(false); 
        }

        private void EnableInterrupt(bool enabled)
        {
            if (enabled)
                encoder.EnableInterrupt();
            else
                encoder.DisableInterrupt();
        }

        public void home()
        {
            _home = 0;
            posFromHome = _home;
            maxDir1 = false;
            maxDir2 = false;
        }

        public int freq
        {
            get { return _freq; }
            set { _freq = value > maxFreq ? maxFreq : value;}
        }

        private void counterRead_OnInterrupt(uint port, uint state, DateTime time)
        {
            encoderCounter++;
        }

        //private void counterRead_OnInterruptDirectional(uint port, uint state, DateTime time)
        //{
        //    encoderCounter++;

        //    posFromHome = dir.Read() ? posFromHome+1 : posFromHome-1;

        //    if (posFromHome >= maxEncoderLimit)
        //    {
        //        //Stop();
        //        maxDir2 = false;
        //        maxDir1 = true;
                
        //    }
        //    else if (posFromHome <= _home)
        //    {
        //       // Stop();
        //        maxDir2 = true;
        //        maxDir1 = false;
                
        //    }
        //    else
        //    {
        //        maxDir2 = false;
        //        maxDir1 = false;
        //    }//end if 
                   
        //}

        //private void counterRead_OnInterrupt_biDirectional(uint port, uint state, DateTime time)
        //{
        //    encoderCounter++;

        //    posFromHome = dir.Read() ? posFromHome + 1 : posFromHome - 1;

        //    if (System.Math.Abs(posFromHome) >= maxEncoderLimit)
        //    {
        //        //Stop();
        //        maxDir1 = posFromHome >= 0 ? true : false;
        //        maxDir2 = !maxDir1;
        //    }
        //    else
        //    {
        //        maxDir1 = false;
        //        maxDir2 = false;
        //    }
        //}

        public void Stop()
        {
            pwm.Set(true);
            setLights(false);
            EnableInterrupt(false);
        }

        private void moveMotors(int freqPassed, bool dirPassed, bool withInterrupt, float freqRatio)
        {
            //if (maxDir1 && dirPassed)
            //{
            //    Stop();
            //    return;
            //}
            //else if (maxDir2 && !dirPassed)
            //{
            //    Stop();
            //    return;
            //}

            EnableInterrupt(withInterrupt);

            freq = freqPassed;
            Dir(dirPassed);
            //Debug.Print(dirPassed.ToString());
            setLights(true);
            pwm.Set(false);
            pwm.Set((int)(freq * freqRatio), duty);
        }

        public void Go(int freqPassed, bool withInterrupt)
        {
            moveMotors(freqPassed, dir.Read(), withInterrupt, 1);          
        }

        public void Go(int freqPassed, bool dirPassed, bool withInterrupt)
        {
            moveMotors(freqPassed, dirPassed, withInterrupt,1);
        }

        public void Go(int freqPassed, bool dirPassed, bool withInterrupt, float freqRatio)
        {
            moveMotors(freqPassed, dirPassed, withInterrupt, freqRatio );
        }
        

        public void Go(int freqPassed, bool dirPassed, double count)
        {
            encoderCounter  = 0;
            moveMotors(freqPassed, dirPassed, true,1);
            while (count > encoderCounter)
            {
                //if ((maxDir1 && dirPassed) || (maxDir2 && !dirPassed) )
                //{
                //    Stop();
                //    //return false;
                //}
            }
            Stop();
           // return true;
        }

        public void Enable(bool value)
        {
            enable.Write(value);
        }

        public void Dir(bool value)
        {
            dir.Write(value);
        }

        public void Dir(bool value, float freqratio)
        {
            dir.Write(value);
            int tempFreq = (int)(freq * freqratio);
            Debug.Print("tempFreq: " + tempFreq.ToString() + " freq: " + freq.ToString () + " freqRatio:" + freqratio.ToString ());
            pwm.Set(tempFreq, duty);
        }

         
        private void setLights(bool onOff)
        {
            if (onOff)
            {
                dirRed.Write(dir.Read ());
                dirgreen.Write(!dir.Read ());
            }
            else
            {
                dirRed.Write(false);
                dirgreen.Write(false);
            }//end if
        }

    }
}
