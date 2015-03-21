using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using GHIElectronics.NETMF.Hardware;

namespace MotionControlTest
{
    class AntSelector
    {
        InputPort sel1;
        InputPort sel2;
        InputPort sel3;
        InputPort sel4;

        public AntSelector(FEZ_Pin.Digital sel1, FEZ_Pin.Digital sel2, FEZ_Pin.Digital sel3, FEZ_Pin.Digital sel4)
        {
            this.sel1 = new InputPort((Cpu.Pin)sel1, false, Port.ResistorMode.PullDown);
            this.sel2 = new InputPort((Cpu.Pin)sel2, false, Port.ResistorMode.PullDown);
            this.sel3 = new InputPort((Cpu.Pin)sel3, false, Port.ResistorMode.PullDown);
            this.sel4 = new InputPort((Cpu.Pin)sel4, false, Port.ResistorMode.PullDown);
        }

        public int antMode
        {
            get
            {
                if (sel4.Read() == true)
                {
                    return 4;
                }
                else if (sel3.Read() == true)
                {
                    return 3;
                }
                else if (sel2.Read() == true)
                {
                    return 2;
                }
                else if (sel1.Read() == true)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }//end get
        }
    }
}
