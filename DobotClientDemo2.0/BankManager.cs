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
            db = new DataBaseSQL("ATM.db");
            db.AddUser(name, balance, password);
        }
        
        public bool ChekExistingAmount(int id, int amount)
        {
            db = new DataBaseSQL("ATM.db");
            int existingAmount = db.GetBalance(id);
            if(existingAmount > amount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void InsertAmount(int id, int amount)
        {
            db = new DataBaseSQL("ATM.db");
            int existingAmount = db.GetBalance(id);
            db.UpdateBalanceByID(id, (existingAmount + amount));
        }
    }
}
