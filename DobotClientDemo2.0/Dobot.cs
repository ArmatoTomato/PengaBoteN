using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DobotClientDemo.CPlusDll;

namespace DobotClientDemo
{
    internal class Dobot
    {
        private Pose pose = new Pose();
        private int cubesInBank = 2;
        //Börja med att denifinera hur många kuber som finns i banken 

        enum Position
        {
            //enum till switch case för olika positioner
            moneyPool,
            sensor,
            neutral,
            up,
            down,
            ground,
            moneyPoolSideOfConvBelt
        };
        enum ConveyerDirection
        {
            // precis vad det står, om bandet ska rulla höger elelr vänter i en switch case
            right,
            left
        };

        private UInt64 CP(byte mod, float x, float y, float z, float velocity, UInt64 cmdIndex = 0)
        {
            //hur roboten ska göra sig och med vilken hastighet
            CPCmd pdbCmd;

            pdbCmd.cpMode = mod;
            pdbCmd.x = x;
            pdbCmd.y = y;
            pdbCmd.z = z;
            pdbCmd.velocity = velocity;
            while (true)
            {
                int ret = DobotDll.SetCPCmd(ref pdbCmd, true, ref cmdIndex);
                if (ret == 0)
                    break;
            }

            return cmdIndex;
        }

        

        public void Withdraw(ref UInt64 cmdIndex)
        {
            // om det inte finns några pengar att ta ut ska man inte kunna ta ut pengar
            if (cubesInBank == 0)
            {
                MessageBox.Show("Not enough money in the machine");
                return;
            }

            DobotGoToActionPose((int)Position.moneyPoolSideOfConvBelt, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.moneyPool, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.ground, ref cmdIndex, ref pose);
            DobotPickUp(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.up, ref cmdIndex, ref pose);
            cubesInBank = cubesInBank -  1;
            DobotGoToActionPose((int)Position.moneyPoolSideOfConvBelt, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.neutral, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.sensor, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.down, ref cmdIndex, ref pose);
            DobotDrop(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.up, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.neutral, ref cmdIndex, ref pose);
            DobotMoveBand((int)ConveyerDirection.right, ref cmdIndex, ref pose);
            //roboten går enligt schemat till sina olika positioner
        }

        public void Deposit(ref UInt64 cmdIndex)
        {
            // sätter på laser och börjar rulla bandet för att invänta att ågot går i vägen för sensorn
            DobotDll.SetInfraredSensor(true, 1, 1);
            EMotor e;
            e.index = 0;
            e.speed = 9000;
            e.isEnabled = 1;

            UInt64 temp = cmdIndex;
            DobotDll.SetQueuedCmdStartExec();

            DobotDll.SetEMotor(ref e, true, ref cmdIndex);

            while (temp < cmdIndex)
                DobotDll.GetQueuedCmdCurrentIndex(ref temp);
            //loopen krävs för att säkerställa att alla komandon får sina rätta index och att de hamnar i kön och utförs korrekt
            DobotDll.SetQueuedCmdStopExec();
            byte value = 0;
            int i = 0;
            while (true)
                // för alltid loop tills att det antingen tar för lång tid eller sensorn känenr av något
            {
                DobotDll.GetInfraredSensor(1, ref value);
                if (value == 1)
                {
                    e.isEnabled = 0;
                    DobotDll.SetEMotor(ref e, false, ref cmdIndex);
                    // stänger av rullbandet när den upptäcker något
                    break;
                }
                i++;
                if(i==1000000)
                {
                    // om det tar för lång tid bör funktionen avbrytas. Tänk så har kunden blivit rånad och behövt springa iväg????
                    break;
                }
                
            }
            DobotDll.SetInfraredSensor(false, 1, 1);
            // stänger av sensorn 

            //lägger till en kub nu när maskinen vet att den finns
            cubesInBank = cubesInBank + 1;

            if (cubesInBank > 4)
            {
                cubesInBank = cubesInBank - 1;
                DobotMoveBand((int)ConveyerDirection.right, ref cmdIndex, ref pose);
                return;
                // om det är fullt i maskinen får man bara tillbaka sin pengar
            }

            DobotGoToActionPose((int)Position.sensor, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.down, ref cmdIndex, ref pose);
            DobotPickUp(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.up, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.neutral, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.moneyPoolSideOfConvBelt, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.moneyPool, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.ground, ref cmdIndex, ref pose);
            DobotDrop(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.up, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.moneyPoolSideOfConvBelt, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.neutral, ref cmdIndex, ref pose);
            // rörelsemöntret för att sätta in pengar
        }

        private void DobotGoToActionPose(int mode, ref UInt64 cmdIndex, ref Pose pose)
        {
            DobotDll.SetQueuedCmdStartExec();
            UInt64 temp = cmdIndex;
            // en temp för att senare kunna jämföra med cmdindex och se så att allt i kön har utförts
            
            switch (mode)
            {
                //en switchcase baserat på 
                case 0:
                    {
                        if (cubesInBank > 4)
                        {
                            cubesInBank = cubesInBank - 1;
                            break;
                        }
                        // om det redan är max antal i banken ska det ju inte stoppas in mer

                        switch (cubesInBank)
                        {
                            case 0: { return; }
                                //kontrollera så att det faktiskt finns pengar i banken annars ska det inte gå att ta ut pengar.

                            case 1:
                                {
                                    pose.x = -41.0928f;
                                    pose.y = -234.3042f;
                                    break;
                                }
                            case 2:
                                {
                                    pose.x = -3.5285f;
                                    pose.y = -246.3564f;
                                    break;
                                }
                            case 3:
                                {
                                    pose.x = 18.0972f;
                                    pose.y = -212.3756f;
                                    break;
                                }
                            case 4:
                                {
                                    pose.x = -17.7272f;
                                    pose.y = -192.2734f;
                                    break;
                                }
                        }
                        pose.z = 50;
                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);

                        break;
                    }
                case 1:
                    {
                        pose.x = 146.5381f;
                        pose.y = 162.9993f;
                        pose.z = 50;

                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        break;
                    }
                      
                   case 2:
                    {
                        HOMEParams hOMEParams = new HOMEParams();

                        DobotDll.GetHOMEParams(ref hOMEParams);

                        pose.x = hOMEParams.x;
                        pose.y = hOMEParams.y;
                        pose.z = hOMEParams.z;

                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        break;
                    }
                case 3:
                    {
                        pose.z = 49;

                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        break;
                    }
                case 4:
                    {
                        pose.z = 13;

                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        break;
                    }
                case 5:
                    {
                        pose.z = -43.6f;

                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        break;
                    }
                case 6:
                {

                        pose.x = 158;
                        pose.y = -220;
                        pose.z = 50;

                        cmdIndex = CP((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        break;
                }
            }

            while (temp < cmdIndex)
                DobotDll.GetQueuedCmdCurrentIndex(ref temp);
            //loopen krävs för att säkerställa att alla komandon får sina rätta index och att de hamnar i kön och utförs korrekt
            DobotDll.SetQueuedCmdStopExec();
        }

        private void DobotMoveBand(int direction, ref UInt64 cmdIndex, ref Pose pose)
        {
            double STEP_PER_CRICLE = 360.0 / 1.8 * 10.0 * 16.0;
            double MM_PER_CRICLE = 3.1415926535898 * 36.0;
            UInt32 dist = (uint)(25 * STEP_PER_CRICLE / MM_PER_CRICLE);
            //Måste vara en positiv för avståndet men genom att göra hastigheten nergativ kan man få bandet att åka fram och tillbaka. 

            EMotorS motor = new EMotorS();
            motor.index = 0;
            motor.isEnabled = 1;
            motor.speed = 9000;
            motor.distance = dist * 4;

            UInt64 temp = cmdIndex;
            DobotDll.SetQueuedCmdStartExec();


            switch (direction)
            {
                case 0:
                    {
                        motor.speed = motor.speed * -1;
                        DobotDll.SetEMotorS(ref motor, true, ref cmdIndex);
                        return;
                    }
                case 1:
                    {
                        DobotDll.SetEMotorS(ref motor, true, ref cmdIndex);
                        return;
                    }
            }

            while (temp < cmdIndex)
                DobotDll.GetQueuedCmdCurrentIndex(ref temp);
            //loopen krävs för att säkerställa att alla komandon får sina rätta index och att de hamnar i kön och utförs korrekt
            DobotDll.SetQueuedCmdStopExec();

            motor.isEnabled = 0;
            DobotDll.SetEMotorS(ref motor, false, ref cmdIndex);

        }

        private void DobotDrop(ref UInt64 cmdIndex, ref Pose pose)
        {
            //stänga av sugproppen

            DobotDll.SetEndEffectorSuctionCup(true, false, true, ref cmdIndex);

        }

        private void DobotPickUp(ref UInt64 cmdIndex, ref Pose pose)
        {
            //säta på sugproppen
            DobotDll.SetEndEffectorSuctionCup(true, true, true, ref cmdIndex);

        }

        public void DobotLaserOn()
        {
            //lazer
            DobotDll.SetInfraredSensor(true, 1, 1);

            byte a = 1;
            int sensor = 0;
            while(sensor != 1)
            {
               sensor = DobotDll.GetInfraredSensor(1, ref a); //LÖS PÅ FREDAG!
            }
        }

    }
}
