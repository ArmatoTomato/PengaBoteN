using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DobotClientDemo
{
    internal class BankManager
    {
        DataBaseSQL db;
        public void CreateAccount(string name, int balance, string password)
        {
            db.AddUser(name, balance, password);
        }
    }
}
