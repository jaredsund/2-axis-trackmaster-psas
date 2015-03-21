using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace MotionControlTest
{
    class LED_Indicators
    {
        private OutputPort greenInd;
        private OutputPort yellowInd;
        private OutputPort redInd;
        
        private  enum indColors { green = 1, yellow, red };

        public LED_Indicators(FEZ_Pin.Digital greenPin, FEZ_Pin.Digital yellowPin, FEZ_Pin.Digital redPin)
        {
            this.greenInd = new OutputPort((Cpu.Pin)greenPin, false);
            this.yellowInd = new OutputPort((Cpu.Pin)yellowPin, false);
            this.redInd = new OutputPort((Cpu.Pin)redPin, true);

        }

        public void green()
        {
            updateIndicators(indColors.green);
            return;
        }

        public void yellow()
        {
            updateIndicators(indColors.yellow);
            return;
        }

        public void red()
        {
            updateIndicators(indColors.red);
            return;
        }

        private void updateIndicators(indColors passedColor)
        {
            switch (passedColor)
            {
                case indColors.green:
                    greenInd.Write(true);
                    yellowInd.Write(false);
                    redInd.Write(false);
                    break;
                case indColors.yellow:
                    greenInd.Write(false);
                    yellowInd.Write(true);
                    redInd.Write(false);
                    break;
                case indColors.red:
                    greenInd.Write(false);
                    yellowInd.Write(false);
                    redInd.Write(true);
                    break;
            }//end switch

        }
    }
}
