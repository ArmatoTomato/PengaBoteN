using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
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

    public DataBaseSQL(string databaseName)//Skapar databasen
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

    private void Close()//Stänger :(
    {
        if (_connection != null)
        {
            _connection.Close();
        }
    }

    public void CreateDataBase()
    {
        Open();

        try
        {
            _command.CommandText = "CREATE TABLE ATM(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, balance INT, password TEXT, temp TEXT);";
            _command.ExecuteNonQuery();//Skapar databasen, kommer endast ske om den inte redan finns
        }
        catch (Exception e)
        {
            WriteToFile(e); //Skriver felmedelandet till en fil som man kan använda som felhantering
        }

        Close();
    }

    public void WriteToFile(System.Exception e)//Skriver felmedelandet till Error.txt filen
    {
        try
        {
            StreamWriter sw = new StreamWriter("Error");
            sw.WriteLine(e);
            sw.Close();
        }
        catch (Exception a)
        {

        }
    }

    private void CheckSQLiteVersion()//Hämtar nuvarande version av SQL
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

    public bool AddUser(string name, int balance, string password, string temp)
    {
        Open();//Öppnar vägen tilL SQL
        _command.CommandText = "INSERT INTO ATM (name, balance, password, temp) VALUES (@name, @balance, @password, @temp);";
        //Skapar en commandline i format som SQL kan hantera

        //Bestämmer vad de olika värdena från parameter listan
        SQLiteParameter nameParam = new SQLiteParameter("@name", System.Data.DbType.String);
        SQLiteParameter balanceParam = new SQLiteParameter("@balance", System.Data.DbType.Int32);
        SQLiteParameter passwordParam = new SQLiteParameter("@password", System.Data.DbType.String);
        SQLiteParameter tempParam = new SQLiteParameter("@temp", System.Data.DbType.String);

        nameParam.Value = name;
        balanceParam.Value = balance;
        passwordParam.Value = password;
        tempParam.Value = temp;

        //Lägger till värena i commandline
        _command.Parameters.Add(nameParam);
        _command.Parameters.Add(balanceParam);
        _command.Parameters.Add(passwordParam);
        _command.Parameters.Add(tempParam);

        _command.Prepare();
        _command.ExecuteNonQuery();
        Close();

        return true;
    }

    public void UpdateBalanceByID(int id, int balance)
    {
        Open();
        _command.CommandText = "UPDATE ATM SET balance = @balance WHERE id = @id;";

        SQLiteParameter idParam = new SQLiteParameter("@id", System.Data.DbType.Int32);
        SQLiteParameter balanceParam = new SQLiteParameter("@balance", System.Data.DbType.Int32);

        idParam.Value = id;
        balanceParam.Value = balance;

        _command.Parameters.Add(idParam);
        _command.Parameters.Add(balanceParam);

        _command.Prepare();
        _command.ExecuteNonQuery();
        Close();
    }
    public int GetBalance(int id)
    {
        Open();

        SQLiteParameter idParam = new SQLiteParameter("@id", System.Data.DbType.Int32);
        idParam.Value = id;
        _command.Parameters.Add(idParam);

        _command.CommandText = "SELECT balance FROM ATM WHERE id = @id;";
        SQLiteDataReader rdr = _command.ExecuteReader();

        int balance = 0;

        while (rdr.Read())
        {
            balance = rdr.GetInt32(0);
        }

        rdr.Close();
        Close();

        return balance;
    }

    public int GetId(string temp)
    {
        Open();

        SQLiteParameter tempParam = new SQLiteParameter("@temp", System.Data.DbType.String);
        tempParam.Value = temp;
        _command.Parameters.Add(tempParam);

        _command.CommandText = "SELECT id FROM ATM WHERE temp = @temp;";
        SQLiteDataReader rdr = _command.ExecuteReader();

        int id = 0;
        while (rdr.Read())
        {
            id = rdr.GetInt32(0);
        }

        rdr.Close();
        Close();

        return id;
    }

    public string GetNameById(int id)//Hämtar namn genom ID
    {
        Open();

        SQLiteParameter idParam = new SQLiteParameter("@id", System.Data.DbType.Int32);
        idParam.Value = id;
        _command.Parameters.Add(idParam);

        _command.CommandText = "SELECT name FROM ATM WHERE id = @id;";
        SQLiteDataReader rdr = _command.ExecuteReader();

        string name = "";//Om inget namn hämtas är name tomt
        while (rdr.Read())
        {
            name = rdr.GetString(0);
        }

        rdr.Close();
        Close();

        return name;
    }
    public void RemoveTemp(int id)//Tar bort den temporära guiden för att spara plats
    {
        Open();
        string temp = "";
        SQLiteParameter idParam = new SQLiteParameter("@id", System.Data.DbType.Int32);
        SQLiteParameter tempParam = new SQLiteParameter("@temp", System.Data.DbType.String);
        idParam.Value = id;
        tempParam.Value = temp;
        _command.Parameters.Add(idParam);
        _command.Parameters.Add(tempParam);

        _command.CommandText = "UPDATE ATM SET temp = @temp WHERE id = @id;";

        _command.Prepare();
        _command.ExecuteNonQuery();

        Close();
    }
    public string GetName(int id)
    {
        Open();

        SQLiteParameter idParam = new SQLiteParameter("@id", System.Data.DbType.Int32);
        idParam.Value = id;
        _command.Parameters.Add(idParam);

        _command.CommandText = "SELECT name FROM ATM WHERE id = @id;";
        SQLiteDataReader rdr = _command.ExecuteReader();

        string name = null;

        while (rdr.Read())
        {
            name = rdr.GetString(0);
        }

        rdr.Close();
        Close();

        return name;
    }

    public string GetPassword(int id)
    {
        Open();

        SQLiteParameter idParam = new SQLiteParameter("@id", System.Data.DbType.Int32);
        idParam.Value = id;
        _command.Parameters.Add(idParam);

        _command.CommandText = "SELECT password FROM ATM WHERE id = @id;";
        SQLiteDataReader rdr = _command.ExecuteReader();

        string password = null;

        while (rdr.Read())
        {
            password = rdr.GetString(0);
        }

        rdr.Close();
        Close();

        return password;
    }
}