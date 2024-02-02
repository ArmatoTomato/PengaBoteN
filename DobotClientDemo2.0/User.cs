using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DobotClientDemo
{
    internal class User
    {
        private string _name { get; set; }
        private string _password { get; set; }
        private int _id { get; set; }

        private User (string name, string password, int id)
        {
            _name = name;
            _password = password;
            _id = id;
        }


        public void LogIn()
        {

        }

        //public void CreateAccount()
        //{
        //    if (Name_Box.Text != "" && Password_Box.Text != "")
        //    {
        //           db.AddStudentToStudentTable(Name_Box.Text, age, SurName_Box.Text);

        //           Name_Box.Clear();
        //           Password_Box.Clear();
        //    }
        //}

    }
}
