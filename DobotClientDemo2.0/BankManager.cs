using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DobotClientDemo
{
    internal class BankManager//Klass som hanterar bank relaterade metoder och vissa användar funtkioner
    {
        DataBaseSQL db;

        public void CreateAccount(string name, int balance, string password, string temp)//Skapar kontot
        {
            db = new DataBaseSQL("ATM.db");
            db.AddUser(name, balance, password, temp);
        }
        
        public bool ChekExistingAmount(int id, int amount)//Kollar hur mycket pengar som finns och om uttaget är möjlugt
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
        public void WithdrawAmount(int id, int amount)//Updaterar saldot
        {
            db = new DataBaseSQL("ATM.db");
            int existingAmount = db.GetBalance(id);
            db.UpdateBalanceByID(id, (existingAmount - amount));
        }

        public int GetBalance(int id)
        {
            db = new DataBaseSQL("ATM.db");
            int balance = db.GetBalance(id);
            return balance;
        }
        public void DepositAmount(int id, int amount)//Motsatt withdraw
        {
            db = new DataBaseSQL("ATM.db");
            int existingAmount = db.GetBalance(id);
            db.UpdateBalanceByID(id, (existingAmount + amount));
        }
    }
}
