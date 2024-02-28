using DobotClientDemo.CPlusDll;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DobotClientDemo
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private byte isJoint = (byte)0;
        private bool isConnectted = false;
        private JogCmd currentCmd;
        private Pose pose = new Pose();
        private System.Timers.Timer posTimer = new System.Timers.Timer();
        DataBaseSQL db;
        BankManager bm;
        int ID { get; set; }
        public MainWindow()
        {

            db = new DataBaseSQL("ATM.db");
            db.CreateDataBase();
            db.UpdateBalanceByID(200, 1);
            bm = new BankManager();

            EncryptionList<string> nd;
            nd = new EncryptionList<string>();
            //List<string> oga = nd.Encrypt("ALva");
            ///attach event handler to corresponding events
            InitializeComponent();
            
            sld.Value = 30;
            sld.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(blurSlider_MouseLeftButtonUp), true);
            //sld1.Value = 30;
            //sld1.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(blurSlider_MouseLeftButtonUp), true);
            //sldAcc.Value = 30;
            //sldAcc.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(blurSlider_MouseLeftButtonUp), true);

            //modeStyle.SelectedIndex = 2;


            //ConsoleManager.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //StartGetPose();

            //StartDobot();
        }

        /// <summary>
        /// StartDobot
        /// </summary>
        private void StartDobot()
        {
            StringBuilder fwType = new StringBuilder(60);
            StringBuilder version = new StringBuilder(60);
            ///连接dobot
            ///connect dobot
            int ret = DobotDll.ConnectDobot("", 115200, fwType, version);
            if (ret != (int)DobotConnect.DobotConnect_NoError)
            {
                Msg("Connect error", MsgInfoType.Error);
                return;
            }
            Msg("Connect success", MsgInfoType.Info);

            isConnectted = true;
            DobotDll.SetCmdTimeout(3000);

            ///获取dobot的名称
            ///get device name and device Serial Number
            string deviceName = "Dobot Magician";
            DobotDll.SetDeviceName(deviceName);
            StringBuilder deviceSN = new StringBuilder(64);
            DobotDll.GetDeviceName(deviceSN, 64);
            ///清理队列并开始执行队列
            ///clear queue and start executing queue
            DobotDll.SetQueuedCmdClear();
            DobotDll.SetQueuedCmdStartExec();
            SetParam();
            AlarmTest();
            
        }
        private void SetParam()
        {
            ///设置运动参数
            ///set motion parameters
            UInt64 cmdIndex = 0;
            JOGJointParams jsParam;
            jsParam.velocity = new float[] { 200, 200, 200, 200 };
            jsParam.acceleration = new float[] { 200, 200, 200, 200 };
            DobotDll.SetJOGJointParams(ref jsParam, false, ref cmdIndex);

            JOGCommonParams jdParam;
            jdParam.velocityRatio = 100;
            jdParam.accelerationRatio = 100;
            DobotDll.SetJOGCommonParams(ref jdParam, false, ref cmdIndex);

            PTPJointParams pbsParam;
            pbsParam.velocity = new float[] { 200, 200, 200, 200 };
            pbsParam.acceleration = new float[] { 200, 200, 200, 200 };
            DobotDll.SetPTPJointParams(ref pbsParam, false, ref cmdIndex);

            PTPCoordinateParams cpbsParam;
            cpbsParam.xyzVelocity = 100;
            cpbsParam.xyzAcceleration = 100;
            cpbsParam.rVelocity = 100;
            cpbsParam.rAcceleration = 100;
            DobotDll.SetPTPCoordinateParams(ref cpbsParam, false, ref cmdIndex);

            PTPJumpParams pjp;
            pjp.jumpHeight = 20;
            pjp.zLimit = 100;
            DobotDll.SetPTPJumpParams(ref pjp, false, ref cmdIndex);

            PTPCommonParams pbdParam;
            pbdParam.velocityRatio = 30;
            pbdParam.accelerationRatio = 30;
            DobotDll.SetPTPCommonParams(ref pbdParam, false, ref cmdIndex);
        }

        private void EIOTest()
        {
            UInt64 cmdIndex = 0;

            //EIO
            IOMultiplexing iom;
            iom.address = 2;//io index
            iom.multiplex = (byte)IOFunction.IOFunctionDO; // ioType
            DobotDll.SetIOMultiplexing(ref iom, false, ref cmdIndex);

            IODO iod;
            iod.address = 2;
            iod.level = 1; // set io index 2 to open
            DobotDll.SetIODO(ref iod, false, ref cmdIndex);
        }

        private void ARCTest()
        {
            ptp((byte)1, 67.99f, 216.4f, -27.99f, 0);
            arc(154.92f, 182.41f, -62.89f, 0f, 216.07f, 85.54f, -8.34f, 0f);
        }

        private void AlarmTest()
        {
            int ret;
            byte[] alarmsState = new byte[32];
            UInt32 len = 32;
            ret = DobotDll.GetAlarmsState(alarmsState, ref len, alarmsState.Length);
            for (int i = 0; i < alarmsState.Length; i++)
            {
                byte alarm = alarmsState[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((alarm & 0x01 << j) > 0)
                    {
                        int alarmIndex = i * 8 + j;
                        switch (alarmIndex)
                        {
                            case 0x00:
                                { // reset
                                    //Get Alarm status: reset
                                    break;
                                }
                            /* other status*/
                            default:
                                break;
                        }
                    }
                }
            }
            //DobotDll.ClearAllAlarmsState();
        }

        /// <summary>
        /// StartPeriodic
        /// </summary>
        private void StartGetPose()
        {
            posTimer.Elapsed += new System.Timers.ElapsedEventHandler(PosTimer_Tick);
            posTimer.Interval = 600;
            posTimer.Start();
        }

        private void PosTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!isConnectted)
                return;

            DobotDll.GetPose(ref pose);

            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                tbJoint1Angle.Text = pose.jointAngle[0].ToString();
                tbJoint2Angle.Text = pose.jointAngle[1].ToString();
                tbJoint3Angle.Text = pose.jointAngle[2].ToString();
                tbJoint4Angle.Text = pose.jointAngle[3].ToString();

                if (sync.IsChecked == true)
                {
                    X.Text = pose.x.ToString();
                    Y.Text = pose.y.ToString();
                    Z.Text = pose.z.ToString();
                    rHead.Text = pose.rHead.ToString();
                    pauseTime.Text = "0";
                }
            });
        }

        private UInt64 ptp(byte style, float x, float y, float z, float r)
        {
            PTPCmd pdbCmd;
            UInt64 cmdIndex = 0;

            pdbCmd.ptpMode = style;
            pdbCmd.x = x;
            pdbCmd.y = y;
            pdbCmd.z = z;
            pdbCmd.rHead = r;
            while(true)
            {
                int ret = DobotDll.SetPTPCmd(ref pdbCmd, true, ref cmdIndex);
                if (ret == 0)
                    break;
            }

            return cmdIndex;
        }

        private UInt64 arc(float x, float y, float z, float r, float x1, float y1, float z1, float r1)
        {
            UInt64 cmdIndex = 0;

            ARCCmd arcCmd;
            arcCmd.cirPoint_x = x;
            arcCmd.cirPoint_y = y;
            arcCmd.cirPoint_z = z;
            arcCmd.cirPoint_r = r;

            arcCmd.toPoint_x = x1;
            arcCmd.toPoint_y = y1;
            arcCmd.toPoint_z = z1;
            arcCmd.toPoint_r = r1;
            while (true)
            {
                int ret = DobotDll.SetARCCmd(ref arcCmd, true, ref cmdIndex);
                if (ret == 0)
                    break;
            }

            return cmdIndex;
        }

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

        // event handle
        private void ProcessEvt(object sender, EventArgs e)
        {
            if (!isConnectted)
                return;

            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;

            float x, y, z, r, gripper, pTime;

            if (!float.TryParse(X.Text, out x) || !float.TryParse(Y.Text, out y) || !float.TryParse(Z.Text, out z) || !float.TryParse(rHead.Text, out r)
                || !float.TryParse(isGripper.Text, out gripper) || !float.TryParse(pauseTime.Text, out pTime))
            {
                Msg("Please input float formate", MsgInfoType.Error);
                return;
            }
            Msg("", MsgInfoType.Info);

            switch (con)
            {
                case "SendPlaybackCmd":
                    {
                        obj.IsEnabled = false;
                        cmdIndex = ptp((byte)modeStyle.SelectedIndex, x, y, z, r);
                        while (true)
                        {
                            UInt64 retIndex = 0;
                            int ind = DobotDll.GetQueuedCmdCurrentIndex(ref retIndex);
                            if (ind == 0 && cmdIndex <= retIndex)
                            {
                                obj.IsEnabled = true;
                                break;
                            }
                        }

                        float waitTime = 0;
                        if (float.TryParse(pauseTime.Text, out waitTime) && waitTime > 0)
                        {
                            WAITCmd waitcmd;
                            waitcmd.timeout = (uint)waitTime;
                            DobotDll.SetWAITCmd(ref waitcmd, false, ref cmdIndex);
                        }
                    }
                    break;
                case "SendCPCmd":
                    {
                        cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100);
                        while (true)
                        {
                            UInt64 retIndex = 0;
                            int ind = DobotDll.GetQueuedCmdCurrentIndex(ref retIndex);
                            if (ind == 0 && cmdIndex <= retIndex)
                            {
                                obj.IsEnabled = true;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        // control event handle
        private void OnEvent(object sender, MouseButtonEventArgs e)
        {
            if (!isConnectted)
                return;

            UInt64 cmdIndex = 0;
            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            switch(con)
            {
                case "X+":
                case "Joint1+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogAPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "X-":
                case "Joint1-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogANPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Y+":
                case "Joint2+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogBPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Y-":
                case "Joint2-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogBNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Z+":
                case "Joint3+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogCPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Z-":
                case "Joint3-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogCNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "R+":
                case "Joint4+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogDPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "R-":
                case "Joint4-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogDNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Gripper+":
                    {
                        
                    }
                    break;
                case "Gripper-":
                    {
                        
                    }
                    break;
                default: 
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isConnectted)
                return;

            ComboBox obj = (ComboBox)sender;
            String tag = obj.Tag.ToString();
            if(tag == "mode")
            {
                bool isJ = ((ComboBoxItem)obj.SelectedItem).Content.ToString() == "Axis";
                isJoint = isJ ? (byte)1 : (byte)0;
                if(isJ)
                {
                    XI.Content = "Joint1+";
                    YI.Content = "Joint2+";
                    ZI.Content = "Joint3+";
                    RI.Content = "Joint4+";

                    XN.Content = "Joint1-";
                    YN.Content = "Joint2-";
                    ZN.Content = "Joint3-";
                    RN.Content = "Joint4-";
                }
                else
                {
                    XI.Content = "X+";
                    YI.Content = "Y+";
                    ZI.Content = "Z+";
                    RI.Content = "R+";

                    XN.Content = "X-";
                    YN.Content = "Y-";
                    ZN.Content = "Z-";
                    RN.Content = "R-";
                }
            }
            else if(tag == "headType")
            {
                string str = ((ComboBoxItem)obj.SelectedItem).Content.ToString();
                if (str == "SuctionCup")
                {
                    cbGrab.IsEnabled = false;
                    cbLaser.IsEnabled = false;
                    cbSuctionCup.IsEnabled = true;
                    EndTypeParams endType;
                    endType.xBias = 59.7f;
                    endType.yBias = 0;
                    endType.zBias = 0;
                    DobotDll.SetEndEffectorParams(ref endType);
                }
                else if (str == "Gripper")
                {
                    cbGrab.IsEnabled = true;
                    cbLaser.IsEnabled = false;
                    cbSuctionCup.IsEnabled = false;
                    EndTypeParams endType;
                    endType.xBias = 59.7f;
                    endType.yBias = 0;
                    endType.zBias = 0;
                    DobotDll.SetEndEffectorParams(ref endType);
                }
                else if (str == "Laser")
                {
                    cbGrab.IsEnabled = false;
                    cbLaser.IsEnabled = true;
                    cbSuctionCup.IsEnabled = false;
                    EndTypeParams endType;
                    endType.xBias = 70f;
                    endType.yBias = 0;
                    endType.zBias = 0;
                    DobotDll.SetEndEffectorParams(ref endType);
                }
                
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DobotDll.DisconnectDobot();
        }

        private void Msg(string str, MsgInfoType infoType)
        {
            lbTip.Content = str;
            switch (infoType)
            {
                case MsgInfoType.Error:
                    lbTip.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case MsgInfoType.Info:
                    lbTip.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                default:
                    break;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!isConnectted)
                return;

            CheckBox obj = (CheckBox)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            if (con == "Grab") // grab
            {
                DobotDll.SetEndEffectorGripper(true, true, false, ref cmdIndex);
            }
            else if (con == "Laser") // Shutting
            {
                DobotDll.SetEndEffectorLaser(true, true, false, ref cmdIndex);
            }
            else if (con == "SuctionCup")
            {
                DobotDll.SetEndEffectorSuctionCup(true, true, false, ref cmdIndex);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isConnectted)
                return;

            CheckBox obj = (CheckBox)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            if (con == "Grab") // cancel grab
            {
                DobotDll.SetEndEffectorGripper(true, false, false, ref cmdIndex);
            }
            else if (con == "Laser") // release laser
            {
                DobotDll.SetEndEffectorLaser(false, false, false, ref cmdIndex);
            }
            else if (con == "SuctionCup")
            {
                DobotDll.SetEndEffectorSuctionCup(false, false, false, ref cmdIndex);
            }
        }

        private void blurSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isConnectted)
                return;

            UInt64 cmdIndex = 0;
            Slider obj = (Slider)sender;
            if (obj.Name == "sld")
            {
                JOGCommonParams jdParam;
                jdParam.velocityRatio = (float)sld.Value;
                jdParam.accelerationRatio = 100;
                DobotDll.SetJOGCommonParams(ref jdParam, false, ref cmdIndex);
            }
            else if (obj.Name == "sld1" || obj.Name == "sldAcc") // playback
            {
                PTPCommonParams pbdParam;
                pbdParam.velocityRatio = (float)sld1.Value;
                pbdParam.accelerationRatio = (float)sldAcc.Value;
                DobotDll.SetPTPCommonParams(ref pbdParam, false, ref cmdIndex);
            }
        }




        private void ButtonDeposit_Click(object sender, RoutedEventArgs e)
        {
            bm.InsertAmount(ID, int.Parse(WithdrawAmount.Text));
            if (!isConnectted)
                return;

            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            DobotDll.GetQueuedCmdCurrentIndex(ref cmdIndex);

            float x, y, z, r;

            x = 200;
            y = 10;
            z = 25;
            DobotDll.SetQueuedCmdStartExec();
            UInt64 temp = cmdIndex;
            cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);

            z = 13;

            cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);
            DobotDll.SetEndEffectorSuctionCup(true, true, true, ref cmdIndex);

            x = 10;
            y = -150;
            z = 18;

            cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);

            z = -43;

            cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);
            DobotDll.SetEndEffectorSuctionCup(false, false, true, ref cmdIndex);

            while (temp < cmdIndex)
                DobotDll.GetQueuedCmdCurrentIndex(ref temp);
            //loopen krävs för att säkerställa att alla komandon får sina rätta index och att de hamnar i kön och utförs korrekt
            DobotDll.SetQueuedCmdStopExec();
        }

        private void ButtonWithdraw_Click(object sender, RoutedEventArgs e)
        {

            bool chekAmount = bm.ChekExistingAmount(ID, int.Parse(WithdrawAmount.Text));
            if(chekAmount == true)
            {
                if (!isConnectted)
                    return;

                Button obj = (Button)sender;
                String con = obj.Content.ToString();
                UInt64 cmdIndex = 0;
                DobotDll.GetQueuedCmdCurrentIndex(ref cmdIndex);

                float x, y, z, r;

                x = 10;
                y = -150;
                z = -43;
                DobotDll.SetQueuedCmdStartExec();
                UInt64 temp = cmdIndex;
                cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);
                DobotDll.SetEndEffectorSuctionCup(true, true, true, ref cmdIndex);

                z = 18;

                cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);


                x = 200;
                y = 10;
                z = 25;

                cmdIndex = cp((byte)ContinuousPathMode.CPAbsoluteMode, x, y, z, 100, cmdIndex);
                DobotDll.SetEndEffectorSuctionCup(false, false, true, ref cmdIndex);

                while (temp < cmdIndex)
                    DobotDll.GetQueuedCmdCurrentIndex(ref temp);
                DobotDll.SetQueuedCmdStopExec();
            }
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            ID = int.Parse(IDTextBox.Text);
            string name = NameTextBox.Text;
            string password = PasswordTextBox.Text;

            string RetrievedName = db.GetName(ID);

            EncryptionList<string> n = new EncryptionList<string>();
            string retrivedPassword = n.Decrypt(ID);

            if (RetrievedName == name && retrivedPassword == password)
            {
                ATMWindow.Visibility = Visibility.Visible;
                LoginWindow.Visibility = Visibility.Collapsed;
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            CreateAccountWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
        }
        private void CreateAccount_Click(object sender, EventArgs e)
        {
            CreateAccountWindow.Visibility = Visibility.Visible;
            LoginWindow.Visibility = Visibility.Collapsed;
        }
        private void CreateAccountCreate_Click(object sender, EventArgs e)
        {
            string name = NameTextBoxCreate.Text;
            string password = PasswordTextBoxCreate.Text;

            try
            {

                if (password != "" && name != "")
                {
                    BankManager user = new BankManager();
                    EncryptionList<string> n = new EncryptionList<string>();
                    List<string> encryptedPassword = n.Encrypt(password.ToLower());
                    encryptedPassword.ToString().Trim();
                    string test = string.Join("", encryptedPassword.ToArray());
                    user.CreateAccount(name, 0, test);
                   
                    NameTextBoxCreate.Clear();
                    PasswordTextBoxCreate.Clear();
                }
                ATMWindow.Visibility = Visibility.Visible;
                CreateAccountWindow.Visibility = Visibility.Collapsed;
            }
            finally
            {

            }
        }

    }
}
