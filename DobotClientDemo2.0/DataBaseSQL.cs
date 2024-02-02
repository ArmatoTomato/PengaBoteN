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
        
        Open();

        try
        {
            _command.CommandText = "CREATE TABLE ATM(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, balance INT);";
            _command.ExecuteNonQuery();
        }
        catch(Exception e)
        {
            WriteToFile(e); //Kanske onödig men cool
        }
        
        Close();

        AddUser("Anton", 500);
    }

    public void WriteToFile(System.Exception e)
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

    public string GetName(int id)
        // för password borde kanske vara private 
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
    // för password borde kanske vara private 
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