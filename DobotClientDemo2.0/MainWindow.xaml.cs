using DobotClientDemo.CPlusDll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;


namespace DobotClientDemo
{
    public partial class MainWindow : Window
    {
        private byte isJoint = (byte)0;
        private bool isConnectted = false;
        private JogCmd currentCmd;

        private Pose pose = new Pose();

        Dobot dobot = new Dobot();

        private System.Timers.Timer posTimer = new System.Timers.Timer();
        DataBaseSQL db;
        BankManager bm;
        int ID { get; set; }
        public MainWindow()
        {
            WindowState = WindowState.Maximized;//Fullscreen
            WindowStyle = WindowStyle.None;

            db = new DataBaseSQL("ATM.db");//Skapar ett databas objekt ATM
            db.CreateDataBase();//SKapar databasen i SQL
            bm = new BankManager();//Skapar en ny bankmanager objekt

            EncryptionList<string> el;//Skapar nytt objekt, en lista som sköter kryptering
            el = new EncryptionList<string>();

            //attach event handler to corresponding events
            InitializeComponent();
            
        }


        //Knappen som byter mellan dark och light mode (kan vara färblinds anpassat)
        private void DarkMode(object sender, RoutedEventArgs e)
        {
            Background = Brushes.DarkSlateGray;
            Balance.Foreground = Brushes.White;
            DarksMode.Content = "Light mode";
        }
        private void LightMode(object sender, RoutedEventArgs e)
        {
            Background = Brushes.LightPink;
            Balance.Foreground = Brushes.Black;
            DarksMode.Content = "Dark mode";
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           StartDobot();
            //startar dobotten
        }

        private void StartDobot()
        {
            StringBuilder fwType = new StringBuilder(60);
            StringBuilder version = new StringBuilder(60);
            //connect dobot
            int ret = DobotDll.ConnectDobot("", 115200, fwType, version);
            if (ret != (int)DobotConnect.DobotConnect_NoError)
            {
                Msg("Connect error", MsgInfoType.Error);
                return;
            }
            Msg("Connect success", MsgInfoType.Info);

            isConnectted = true;
            DobotDll.SetCmdTimeout(3000);

            //get device name and device Serial Number
            string deviceName = "Dobot Magician";
            DobotDll.SetDeviceName(deviceName);
            StringBuilder deviceSN = new StringBuilder(64);
            DobotDll.GetDeviceName(deviceSN, 64);
            ///clear queue and start executing queue
            DobotDll.SetQueuedCmdClear();
            DobotDll.SetQueuedCmdStartExec();
            SetParam();
            AlarmTest();
        }
        private void SetParam()
        {
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

            HOMEParams homeParam;

            homeParam.r = 0;
            homeParam.x = 260;
            homeParam.y = 0;
            homeParam.z = 25;
            DobotDll.SetHOMEParams(ref homeParam, false, ref cmdIndex);


            HOMECmd homeCmd = new HOMECmd();
            DobotDll.SetHOMECmd(ref homeCmd, false, ref cmdIndex);
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


        private void ButtonDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnectted)
                return;

            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            DobotDll.GetQueuedCmdCurrentIndex(ref cmdIndex);
            for (int i = 0; i < int.Parse(Amount.Text) / 100; i++)//Körs så många gånger som inmatning/100 för att alla kuber är värda 100
            {
                dobot.Deposit(ref cmdIndex);//Sätter in pengar
            }
            bm.DepositAmount(ID, int.Parse(Amount.Text));//Skickar in insättningen och id till bm som sätter in på rätt konto
            Balance.Text = "Your balance is: " + db.GetBalance(ID).ToString() + "sek";//Skriver ut hur mycket pengar som finns på kontot
        }

        private void ButtonWithdraw_Click(object sender, RoutedEventArgs e)
        {
            bool chekAmount = bm.ChekExistingAmount(ID, int.Parse(Amount.Text));//Kollar om mängden pengar som ska tas ut är möjligt

            if (!isConnectted)
                return;//Är roboten inte ansluten körs inte koden

            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            DobotDll.GetQueuedCmdCurrentIndex(ref cmdIndex);
            if (chekAmount == true)//Om det är möjligt att ta ut den inmatade summan
            {
                for(int i = 0; i < int.Parse(Amount.Text)/100; i++)//En motsatt deposit
                {
                    dobot.Withdraw(ref cmdIndex);
                }
            }
            bm.WithdrawAmount(ID, int.Parse(Amount.Text));
            Balance.Text = "Your balance is: " + db.GetBalance(ID).ToString() + "sek";
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            int o = 0;
            try
            {
                ID = int.Parse(IDTextBox.Text);//Kollar så ID är siffror och inget annat
            }
            catch
            {
                MessageBox.Show("Invalid credentials");
                o = 1;
            }
            if (o == 1)
                return;//Då avslutas funktionen

            int a = 0;
            string nameTry = db.GetNameById(ID);
            //Kontroll om ID finns, om namnet är tomt finns inte konto med inmatade ID
            try
            {
                if (nameTry == "")//Om konto inte finns
                {
                    int.Parse("A");//Kommer aldrig gå och då avslutas funktionen
                }
            }
            catch
            {
                MessageBox.Show("Invalid credentials");
                a = 1;
            }
            if(a == 1)
                return;

            //Tar data från rutorna i gränssnittet
            string name = NameTextBox.Text;
            string password = PasswordTextBox.Text;
            
            string retrievedName = db.GetName(ID);//Hämtar namnet

            EncryptionList<string> n = new EncryptionList<string>();
            string retrivedPassword = n.Decrypt(ID);//Hämtar det dekrypterade lösenordet

            if (retrievedName == name && retrivedPassword == password.ToLower())//Om det stämmer så bytas man till en annan vy
            {
                ATMWindow.Visibility = Visibility.Visible;
                LoginWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Invalid credentials");
            }

            Balance.Text = "Your balance is: " + db.GetBalance(ID).ToString() + "sek";
        }

        private void IDTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //kolla om sender är textbox
            try { 
                ((TextBox)sender).Text = "";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void Back_Click(object sender, EventArgs e)//Bytar vy
        {
            CreateAccountWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
        }
        private void LogOut_Click(object sender, EventArgs e)
        {
            ATMWindow.Visibility= Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
            ID = 0;
        }
        private void CreateAccountMenu_Click(object sender, EventArgs e)//Bytar till skapa konto vyn
        {
            CreateAccountWindow.Visibility = Visibility.Visible;
            LoginWindow.Visibility = Visibility.Collapsed;
        }
        private void CreateAccount_Click(object sender, EventArgs e)
        {
            string name = NameTextBoxCreate.Text;
            string password = PasswordTextBoxCreate.Text;

            try
            {
                if (password != "" && name != "")//Kollar så rutorna innehåller data
                {
                    BankManager user = new BankManager();
                    EncryptionList<string> n = new EncryptionList<string>();
                    List<string> encryptedPasswordList = n.Encrypt(password.ToLower());//Krypterar lösenordert
                    encryptedPasswordList.ToString().Trim();//Tar bort mellanslag
                    string passwordEncrypted = string.Join("", encryptedPasswordList.ToArray());//Gör om till string
                    string guid = Guid.NewGuid().ToString();//Skapar en temporär variabel som används för att hitta ID
                    user.CreateAccount(name, 0, passwordEncrypted, guid);//Skapar kontot
                    int id = db.GetId(guid);//Hämtar ID genom GUID som aldrig kan vara samma två gånger
                    db.RemoveTemp(id); //FIXA DETTA! , Tar bort temporära guid
                    NameTextBoxCreate.Clear();
                    PasswordTextBoxCreate.Clear();

                    MessageBox.Show($"Your ID is {id.ToString()}");
                }
            }
            finally
            {

            }
            CreateAccountWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
        }
    }
}
