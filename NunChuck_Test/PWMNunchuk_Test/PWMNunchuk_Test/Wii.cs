/*
Copyright 2010 GHI Electronics LLC
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;


namespace PWMNunchuk_Test
{
    public static partial class FEZ_Components
    {
        public static class Wii
        {
            /// <summary>
            /// Class to read out Nunchuk trough I2C
            /// (C) 2008, Elze Kool. www.microframework.nl
            /// You may use this source as you like as long as you keep in above reference.
            /// </summary>
            public static class Nunchuk
            {
                // Private properties
                private static Thread ReadNumChuck;
                private static I2CDevice NumChuckDevice;
                private static byte[] RawData = new byte[6];

                private static int _AnalogStickX = 0;
                private static int _AnalogStickY = 0;

                private static int _AccelerateX = 0;
                private static int _AccelerateY = 0;
                private static int _AccelerateZ = 0;

                private static bool _ButtonC = false;
                private static bool _ButtonZ = false;

                /// <summary>
                /// NumChuck connected
                /// </summary>
                public static bool Connected = false;

                /// <summary>
                /// X Position of Analog Stick
                /// </summary>
                public static int AnalogStickX
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogStickX;
                        }
                    }
                }


                /// <summary>
                /// Y Position of Analog Stick
                /// </summary>
                public static int AnalogStickY
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogStickY;
                        }
                    }
                }



                /// <summary>
                /// Force on X Axis from acceleration meter
                /// </summary>
                public static int AccelerateX
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AccelerateX;
                        }
                    }
                }

                /// <summary>
                /// Force on Y Axis from acceleration meter
                /// </summary>
                public static int AccelerateY
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AccelerateY;
                        }
                    }
                }


                /// <summary>
                /// Force on Z Axis from acceleration meter
                /// </summary>
                public static int AccelerateZ
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AccelerateZ;
                        }
                    }
                }

                /// <summary>
                /// Button C pressed
                /// </summary>
                public static bool ButtonC
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonC;
                        }
                    }
                }

                /// <summary>
                /// Button Z pressed
                /// </summary>
                public static bool ButtonZ
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonZ;
                        }
                    }
                }

                /// <summary>
                /// Initialize NumChuck and start update thread
                /// </summary>
                public static void Initialize()
                {
                    // Create I2C Device for NumChuck
                    // Address: 0x52
                    // Clock: 100Khz
                    NumChuckDevice = new I2CDevice(new I2CDevice.Configuration(0x52, 10));

                    // Start thread that monitors NumChuck
                    ReadNumChuck = new Thread(new ThreadStart(ReadNumChuckDataThread));
                    ReadNumChuck.Priority = ThreadPriority.BelowNormal;
                    ReadNumChuck.Start();

                }

                /// <summary>
                /// Reset NumChuck returns true on succes false on error (No NumChuck Connected)
                /// </summary>
                /// <returns>Succes</returns>
                public static bool Reset()
                {
                    // Initialize NumChuck by sending 0x40, 0x00
                    I2CDevice.I2CTransaction[] NumChuckInitializeTrans = new I2CDevice.I2CTransaction[1];
                    NumChuckInitializeTrans[0] = I2CDevice.CreateWriteTransaction(new byte[] { 0x40, 0x00 });
                    if (NumChuckDevice.Execute(NumChuckInitializeTrans, 100) == 0)
                    {
                        Connected = false;
                        return false;
                    }
                    else
                    {
                        Connected = true;
                        return true;
                    }
                }

                /// <summary>
                /// Thread that reads out the NumChuck data and updates public properties
                /// </summary>
                private static void ReadNumChuckDataThread()
                {
                    // Endless proces 
                    while (true)
                    {
                        // Wait until NumChuck is connected
                        if (Connected == false) { while (Reset() == false) { } }

                        // Send 0x00 indicating we want data!
                        I2CDevice.I2CTransaction[] NumChuckSaySendMeData = new I2CDevice.I2CTransaction[1];
                        NumChuckSaySendMeData[0] = I2CDevice.CreateWriteTransaction(new byte[] { 0x00 });
                        if (NumChuckDevice.Execute(NumChuckSaySendMeData, 100) == 0)
                        {
                            // Error, maybe NumChuck was detached
                            Connected = false;
                            continue;
                        }

                        // Now get the Bytes!
                        I2CDevice.I2CTransaction[] NumChuckGetTheData = new I2CDevice.I2CTransaction[1];
                        NumChuckGetTheData[0] = I2CDevice.CreateReadTransaction(RawData);
                        if (NumChuckDevice.Execute(NumChuckGetTheData, 100) != 6)
                        {
                            // Error, maybe NumChuck was detached
                            Connected = false;
                            continue;
                        }
                        else
                        {
                            // Decode NumChuck Bytes
                            for (int x = 0; x < 6; x++) { RawData[x] = (byte)((RawData[x] ^ 0x17) + 0x17); }

                            // Make Updating data Thread Save
                            lock (RawData)
                            {
                                // Analog Stick X/Y
                                _AnalogStickX = RawData[0] - 0x7E;
                                _AnalogStickY = RawData[1] - 0x7B;

                                // Acceleration X/Y/Z
                                _AccelerateX = (((RawData[2] - 0x7D) << 2) | ((RawData[5] >> 2) & 0x03));
                                _AccelerateY = (((RawData[3] - 0x7D) << 2) | ((RawData[5] >> 4) & 0x03));
                                _AccelerateZ = (((RawData[4] - 0x7D) << 2) | ((RawData[5] >> 6) & 0x03));

                                // Buttons
                                _ButtonZ = !((RawData[5] & 0x01) == 0x01);
                                _ButtonC = !((RawData[5] & 0x02) == 0x02);
                            }
                        }
                        // Wait a while, about 1/20th sec
                        Thread.Sleep(50);
                    }
                }
            }

            /// <summary>
            /// Class to read out Wii Classic Controller trough I2C
            /// (C) 2008, Elze Kool. www.microframework.nl
            /// You may use this source as you like as long as you keep in above reference.
            /// </summary>
            static public class ClassicController
            {
                // Private properties
                private static Thread ReadClassicController;
                private static I2CDevice ClassicControllerDevice;
                private static byte[] RawData = new byte[6];

                private static bool _ButtonR = false;
                private static bool _ButtonL = false;

                private static bool _ButtonPlus = false;
                private static bool _ButtonHome = false;
                private static bool _ButtonMinus = false;

                private static bool _ButtonUp = false;
                private static bool _ButtonDown = false;
                private static bool _ButtonLeft = false;
                private static bool _ButtonRight = false;

                private static bool _ButtonZR = false;
                private static bool _ButtonZL = false;

                private static bool _ButtonX = false;
                private static bool _ButtonY = false;

                private static bool _ButtonA = false;
                private static bool _ButtonB = false;


                private static int _AnalogLeftX = 0;
                private static int _AnalogLeftY = 0;

                private static int _AnalogRightX = 0;
                private static int _AnalogRightY = 0;

                private static int _AnalogLeft = 0;
                private static int _AnalogRight = 0;

                /// <summary>
                /// Classic Controller connected
                /// </summary>
                public static bool Connected = false;

                /// <summary>
                /// Button R Fully Pressed
                /// </summary>
                public static bool ButtonR
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonR;
                        }
                    }
                }

                /// <summary>
                /// Button L Fully Pressed
                /// </summary>
                public static bool ButtonL
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonL;
                        }
                    }
                }

                /// <summary>
                /// Button +
                /// </summary>
                public static bool ButtonPlus
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonPlus;
                        }
                    }
                }


                /// <summary>
                /// Button Home
                /// </summary>
                public static bool ButtonHome
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonHome;
                        }
                    }
                }

                /// <summary>
                /// Button -
                /// </summary>
                public static bool ButtonMinus
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonMinus;
                        }
                    }
                }


                /// <summary>
                /// Button Up
                /// </summary>
                public static bool ButtonUp
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonUp;
                        }
                    }
                }

                /// <summary>
                /// Button Down
                /// </summary>
                public static bool ButtonDown
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonDown;
                        }
                    }
                }

                /// <summary>
                /// Button Left
                /// </summary>
                public static bool ButtonLeft
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonLeft;
                        }
                    }
                }

                /// <summary>
                /// Button Right
                /// </summary>
                public static bool ButtonRight
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonRight;
                        }
                    }
                }

                /// <summary>
                /// Button ZR
                /// </summary>
                public static bool ButtonZR
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonZR;
                        }
                    }
                }

                /// <summary>
                /// Button ZL
                /// </summary>
                public static bool ButtonZL
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonZL;
                        }
                    }
                }

                /// <summary>
                /// Button X
                /// </summary>
                public static bool ButtonX
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonX;
                        }
                    }
                }

                /// <summary>
                /// Button Y
                /// </summary>
                public static bool ButtonY
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonY;
                        }
                    }
                }

                /// <summary>
                /// Button A
                /// </summary>
                public static bool ButtonA
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonA;
                        }
                    }
                }

                /// <summary>
                /// Button B
                /// </summary>
                public static bool ButtonB
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _ButtonB;
                        }
                    }
                }

                /// <summary>
                /// Analog Left Stick, X-Axis
                /// </summary>
                public static int AnalogLeftX
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogLeftX;
                        }
                    }
                }

                /// <summary>
                /// Analog Left Stick, Y-Axis
                /// </summary>
                public static int AnalogLeftY
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogLeftY;
                        }
                    }
                }


                /// <summary>
                /// Analog Right Stick, X-Axis
                /// </summary>
                public static int AnalogRightX
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogRightX;
                        }
                    }
                }

                /// <summary>
                /// Analog Right Stick, Y-Axis
                /// </summary>
                public static int AnalogRightY
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogRightY;
                        }
                    }
                }

                /// <summary>
                /// Analog Left Button
                /// </summary>
                public static int AnalogLeft
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogLeft;
                        }
                    }
                }

                /// <summary>
                /// Analog Right Button
                /// </summary>
                public static int AnalogRight
                {
                    get
                    {
                        lock (RawData)
                        {
                            return _AnalogRight;
                        }
                    }
                }

                /// <summary>
                /// Initialize Classic Controller and start update thread
                /// </summary>
                public static void Initialize()
                {
                    // Create I2C Device for Classic Controller
                    // Address: 0x52
                    // Clock: 100Khz
                    ClassicControllerDevice = new I2CDevice(new I2CDevice.Configuration(0x52, 10));

                    // Start thread that monitors NumChuck
                    ReadClassicController = new Thread(new ThreadStart(ReadClassicControllerData));
                    ReadClassicController.Priority = ThreadPriority.BelowNormal;
                    ReadClassicController.Start();
                }

                /// <summary>
                /// Reset Classic Controller, returns true on succes false on error (No Classic Controller  Connected)
                /// </summary>
                /// <returns>Succes</returns>
                public static bool Reset()
                {
                    // Initialize NumChuck by sending 0x40, 0x00
                    I2CDevice.I2CTransaction[] ClassicControllerInitializeTrans = new I2CDevice.I2CTransaction[1];
                    ClassicControllerInitializeTrans[0] = I2CDevice.CreateWriteTransaction(new byte[] { 0x40, 0x00 });
                    if (ClassicControllerDevice.Execute(ClassicControllerInitializeTrans, 100) == 0)
                    {
                        Connected = false;
                        return false;
                    }
                    else
                    {
                        Connected = true;
                        return true;
                    }
                }

                /// <summary>
                /// Thread that reads out the Classic Controller data and updates public properties
                /// </summary>
                private static void ReadClassicControllerData()
                {
                    // Endless proces 
                    while (true)
                    {
                        // Wait until Classic Controller is connected
                        if (Connected == false) { while (Reset() == false) { } }

                        // Send 0x00 indicating we want data!
                        I2CDevice.I2CTransaction[] ClassicControllerSendMeData = new I2CDevice.I2CTransaction[1];
                        ClassicControllerSendMeData[0] = I2CDevice.CreateWriteTransaction(new byte[] { 0x00 });
                        if (ClassicControllerDevice.Execute(ClassicControllerSendMeData, 100) == 0)
                        {
                            // Error, maybe Classic Controller was detached
                            Connected = false;
                            continue;
                        }

                        // Now get the Bytes!
                        I2CDevice.I2CTransaction[] ClassicControllerGetData = new I2CDevice.I2CTransaction[1];
                        ClassicControllerGetData[0] = I2CDevice.CreateReadTransaction(RawData);
                        if (ClassicControllerDevice.Execute(ClassicControllerGetData, 100) != 6)
                        {
                            // Error, maybe Classic Controller was detached
                            Connected = false;
                            continue;
                        }
                        else
                        {
                            // Decode Classic Controller Bytes
                            for (int x = 0; x < 6; x++) { RawData[x] = (byte)((RawData[x] ^ 0x17) + 0x17); }

                            // Make Thread safe
                            lock (RawData)
                            {
                                // Analog Sticks - Byte 0, 1, 2
                                _AnalogLeftX = ((RawData[0] & 0x3F)) - 32;
                                _AnalogLeftY = ((RawData[1] & 0x3F)) - 30;

                                _AnalogRightX = ((((RawData[0] & 0xC0) >> 3) | ((RawData[1] & 0xC0) >> 5) | ((RawData[2] & 0x80) >> 7)) - 15) * 2;
                                _AnalogRightY = (((RawData[2] & 0x1F)) - 16) * 2;

                                // Left/Right Analog Buttons - Byte 2, 3
                                _AnalogLeft = ((RawData[2] & 0x60) >> 2) | ((RawData[3] & 0xE0) >> 5);
                                _AnalogRight = (RawData[3] & 0x1F);

                                // Process Raw Data - Byte 4
                                _ButtonR = !((RawData[4] & 0x02) == 0x02);
                                _ButtonPlus = !((RawData[4] & 0x04) == 0x04);
                                _ButtonHome = !((RawData[4] & 0x08) == 0x08);
                                _ButtonMinus = !((RawData[4] & 0x10) == 0x10);
                                _ButtonL = !((RawData[4] & 0x20) == 0x20);
                                _ButtonDown = !((RawData[4] & 0x40) == 0x40);
                                _ButtonRight = !((RawData[4] & 0x80) == 0x80);

                                // Process Raw Data - Byte 5
                                _ButtonUp = !((RawData[5] & 0x01) == 0x01);
                                _ButtonLeft = !((RawData[5] & 0x02) == 0x02);
                                _ButtonZR = !((RawData[5] & 0x04) == 0x04);
                                _ButtonX = !((RawData[5] & 0x08) == 0x08);
                                _ButtonA = !((RawData[5] & 0x10) == 0x10);
                                _ButtonY = !((RawData[5] & 0x20) == 0x20);
                                _ButtonB = !((RawData[5] & 0x40) == 0x40);
                                _ButtonZL = !((RawData[5] & 0x80) == 0x80);
                            }
                        }
                        // Wait a while, about 1/20th sec
                        Thread.Sleep(50);
                    }
                }
            }
        }
    }
}
