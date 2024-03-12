﻿using System;
using System.Collections.Generic;
using System.Linq;
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


        enum Position
        {
            fack,
            sensor,
            neutral,
            upp,
            ner
        };
        enum ConveyerDirection
        {
            right,
            left
        };

        private UInt64 cp(byte mod, float x, float y, float z, float velocity, UInt64 cmdIndex = 0)
        {
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

            DobotGoToActionPose((int)Position.fack, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.ner, ref cmdIndex, ref pose);
            DobotPickUp(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.upp, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.sensor, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.ner, ref cmdIndex, ref pose);
            DobotDrop(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.upp, ref cmdIndex, ref pose);
            DobotDrop(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.neutral, ref cmdIndex, ref pose);
            DobotMoveBand((int)ConveyerDirection.right, ref cmdIndex, ref pose);



        }

        public void Deposit(ref UInt64 cmdIndex)
        {

            DobotGoToActionPose((int)Position.sensor, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.ner, ref cmdIndex, ref pose);
            DobotPickUp(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.upp, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.fack, ref cmdIndex, ref pose);
            DobotMoveBand((int)ConveyerDirection.left, ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.ner, ref cmdIndex, ref pose);
            DobotDrop(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.upp, ref cmdIndex, ref pose);
            DobotDrop(ref cmdIndex, ref pose);
            DobotGoToActionPose((int)Position.neutral, ref cmdIndex, ref pose);


        }


        private void DobotGoToActionPose(int mode, ref UInt64 cmdIndex, ref Pose pose)
        {

            UInt64 temp = cmdIndex;
            
            switch (mode)
            {
                case 0:
                    {

                        pose.x = 200;
                        pose.y = -150;
                        pose.z = 50;

                        cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        return;
                    }
                case 1:
                    {
                        pose.x = 177;
                        pose.y = 122;
                        pose.z = 50;

                        cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);

                        return;
                    }
                   case 2:
                    {

                        pose.x = 200;
                        pose.y = 0;
                        pose.z = 50;

                        cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        return;
                    }
                case 3:
                    {
                        pose.z = 50;

                        cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        return;
                    }
                case 4:
                    {
                        pose.z = 14;

                        cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, pose.x, pose.y, pose.z, 100, cmdIndex);
                        return;
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

            EMotorS motor = new EMotorS();
            motor.index = 0;
            motor.isEnabled = 1;
            motor.speed = 9000;
            motor.distance = dist * 2 ;

            switch (direction)
            {
                case 0:
                    {
                        motor.speed = motor.speed * -1;
                        DobotDll.SetEMotorS(ref motor, false, ref cmdIndex);
                        return;
                    }
                case 1:
                    {
                        DobotDll.SetEMotorS(ref motor, false, ref cmdIndex);
                        return;
                    }
            }

            motor.isEnabled = 0;
            DobotDll.SetEMotorS(ref motor, false, ref cmdIndex);
        }

        private void DobotDrop(ref UInt64 cmdIndex, ref Pose pose)
        {

            DobotDll.SetEndEffectorSuctionCup(true, false, true, ref cmdIndex);

        }

        private void DobotPickUp(ref UInt64 cmdIndex, ref Pose pose)
        {
            DobotDll.SetEndEffectorSuctionCup(true, true, true, ref cmdIndex);

        }

        //private void DobotMoveBand(ref UInt64 cmdIndex)
        //{

        //    double STEP_PER_CRICLE = 360.0 / 1.8 * 10.0 * 16.0;
        //    double MM_PER_CRICLE = 3.1415926535898 * 36.0;
        //    UInt32 dist = (uint)(25 * STEP_PER_CRICLE / MM_PER_CRICLE);

        //    EMotorS motor = new EMotorS();
        //    motor.index = 0;
        //    motor.isEnabled = 1;
        //    motor.speed = -9000;
        //    motor.distance = dist;
        //    int s = DobotDll.SetEMotorS(ref motor, false, ref cmdIndex);

        //    motor.isEnabled = 0;
        //    DobotDll.SetEMotorS(ref motor, false, ref cmdIndex);

        //}
    }
}