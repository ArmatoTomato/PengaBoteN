using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class DataBaseSQL
{
    string _database_name;
    string _connection_string;
    SQLiteConnection _connection; //kräver using System.Data.SQLite;
    SQLiteCommand _command;

    public DataBaseSQL(string databaseName)
	{
        _database_name = databaseName;
        _connection_string = "URI=file:" + databaseName;
        _connection = new SQLiteConnection(_connection_string);

        CheckSQLiteVersion();
        GetCurrentTime();
    }

    private void Open() //Öppnar portalen
    {
        if (_connection != null)
        {
            _connection.Open();
        }
    }

    private void Close()
    {
        if (_connection != null)
        {
            _connection.Close();
        }
    }

    public void CreateDataBase()
    {
        if (WriteToFile() == true)
        {
            Open();
            _command.CommandText = "CREATE TABLE ATM(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, balance INT);";
            _command.ExecuteNonQuery();
        }
        //TRY CATCH ISTÄLLET KMS

        Close();

        AddUser("Anton", 500);
    }

    public bool WriteToFile()
    {
        bool test;

        try
        {
            StreamWriter sw = new StreamWriter("ChekIfRan");
            sw.WriteLine(1);
            sw.Close();
            test = true;
        }
        catch (Exception e)
        {
            test = false;
        }

        return test;
    }

    private static bool ReadFile()
    {
        List<string> rowsInFile = new List<string>();
        String line;
        try
        {
            StreamReader sr = new StreamReader("ChekIfRan");
            line = sr.ReadLine();

            while (line != null)
            {
                line = sr.ReadLine();
                rowsInFile.Add(line);
            }

            sr.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        finally
        {

        }
        if(rowsInFile.Contains("1"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    private void CheckSQLiteVersion()
    {
        Open();
        string stm = "SELECT SQLITE_VERSION();";
        _command = new SQLiteCommand(stm, _connection);
        string version = _command.ExecuteScalar().ToString();

        Close();
    }

    private void GetCurrentTime()
    {
        Open();
        string stm = "SELECT(datetime('now', 'localtime'));";
        _command = new SQLiteCommand(stm, _connection);
        string date = _command.ExecuteScalar().ToString();
        Close();
    }





    public bool AddUser(string name, int balance)
    {
        Open();
        _command.CommandText = "INSERT INTO ATM (name, balance) VALUES (@name, @balance);";

        SQLiteParameter nameParam = new SQLiteParameter("@name", System.Data.DbType.String);
        SQLiteParameter balanceParam = new SQLiteParameter("@balance", System.Data.DbType.Int32);

        nameParam.Value = name;
        balanceParam.Value = balance;

        _command.Parameters.Add(nameParam);
        _command.Parameters.Add(balanceParam);

        _command.Prepare();
        _command.ExecuteNonQuery();

        Close();

        return true;
    }
}