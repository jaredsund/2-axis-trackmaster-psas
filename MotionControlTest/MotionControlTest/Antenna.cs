using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace MotionControlTest
{
    class Antenna
    {
        public class status //provides access to the current stored values of the antenna array
        {
            public int highestAnt;
            public  int deltaY;
            public  int deltaX;
            public  int delta;
            public  float deltaRatio;

            public bool xDir;
            public bool yDir;

            public string message
            {
                get{return highestAnt.ToString() + " deltaY:" + deltaY.ToString() + " deltaX:" + deltaX.ToString() + " delta:" + delta.ToString() + " deltaRatio: " + deltaRatio.ToString();}
            }
        }

        public status Status;
   
        private const int arraySize = 4;            //Constant for the size of the arrays
        private AnalogIn [] antArray;               //this array holds all the AnalogInputs
        private int[] antBase;                      //array for base antenna values
        private int[] antSample;                    //array for a give sample of antenna values
        
        private const int linearScaleMin = 0;       //set the minimum scale on the analog inputs
        private const int linearScaleMax = 3300;    //set the maximum scale on the analog inputs (milliVolts)

        //constuctor
        public Antenna(FEZ_Pin.AnalogIn antLeg1, FEZ_Pin.AnalogIn antLeg2, FEZ_Pin.AnalogIn antLeg3, FEZ_Pin.AnalogIn antLeg4)
        {
            antArray = new AnalogIn[arraySize];
            antArray[0] = new AnalogIn((AnalogIn.Pin)antLeg1); antArray[0].SetLinearScale(linearScaleMin, linearScaleMax);
            antArray[1] = new AnalogIn((AnalogIn.Pin)antLeg2); antArray[1].SetLinearScale(linearScaleMin, linearScaleMax);
            antArray[2] = new AnalogIn((AnalogIn.Pin)antLeg3); antArray[2].SetLinearScale(linearScaleMin, linearScaleMax);
            antArray[3] = new AnalogIn((AnalogIn.Pin)antLeg4); antArray[3].SetLinearScale(linearScaleMin, linearScaleMax);

            antBase = new int[arraySize];
            antSample = new int[arraySize];
            Status = new status();

            Thread AntennaControlThread = new Thread(poleAntennas);
            AntennaControlThread.Start();
        }//end of constructor


        private void poleAntennas()
        {
            while (true)
            {
                genValues();
                Thread.Sleep(20);
            }
        }

        public void setBase()
        {
            for (int i = 0; i < arraySize; i++)
            {
                antBase[i] = antArray[i].Read();
            }//end for loop
        }//end of function setBase

        public string AntVoltage(int antNum)
        {
            if (antNum <= arraySize && antNum >= 0)
                return antArray[antNum].Read().ToString();
            else
                return "";
        }//end of function AntVoltage(int antNum)

        public string AntVoltage()
        {
            string returnString = "";
            for (int i = 0; i < arraySize; i++)
            {
                returnString += " Ant" + (i + 1).ToString() + ":" + antArray[i].Read().ToString();
            }//end of for loop

            return returnString;
        }//end of function AntVoltage()

        public void genValues()
        {
            int i = 0;  //loop indexing values

            //fill sample array
            for (i = 0; i < arraySize; i++)
            {
                antSample[i] = antArray[i].Read();
            }//end for loop
  
            //calculate the deltas
            int dY1 = ((antSample[0] - antSample[3]));
            int dY2 = ((antSample[1] - antSample[3]));
            int dX1 = ((antSample[0] - antSample[1]));
            int dX2 = ((antSample[2] - antSample[3]));

            Status.deltaY = System.Math .Abs ( dY1) > System.Math.Abs ( dY2) ? dY1 : dY2;
            Status.deltaX = System.Math.Abs(dX1) > System.Math.Abs(dX2) ? dX1 : dX2;

            Status.delta = System.Math.Abs(Status.deltaY) > System.Math.Abs(Status.deltaX) ? Status.deltaY : Status.deltaX;

            Status.deltaRatio = ((float)Status.delta / (float)linearScaleMax);
            if (Status.deltaRatio < 0)
                Status.deltaRatio = Status.deltaRatio * -1;

            //determine the highest value antenna
            if (Status.deltaX >= 0 && Status.deltaY >= 0)
            {
                Status.highestAnt = 1;
                Status.yDir = true;
                Status.xDir = false;
            }
            else if (Status.deltaX < 0 && Status.deltaY >= 0)
            {
                Status.highestAnt = 2;
                Status.yDir = true;
                Status.xDir = true;
            }
            else if (Status.deltaX >= 0 && Status.deltaY < 0)
            {
                Status.highestAnt = 3;
                Status.yDir = false;
                Status.xDir = false;
            }
            else
            {
                Status.highestAnt = 4;
                Status.yDir = false;
                Status.xDir = true;
            }//end if

        }//end of funtion genValues()

    }//end of class
}//end of namespace
