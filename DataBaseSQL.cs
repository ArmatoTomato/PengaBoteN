using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DataBaseSQL
{
    string? _database_name;
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
        bool test;

        static void WriteToFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter("ChekIfRan");
                sw.WriteLine(1);
                sw.Close();
            }
            catch (Exception e)
            {
                test = false;
            }  
            finally
            {
                test = true;
            }

            return test;
        }

        if (test = true && ReadFile == true)
        {
            Open();
            _command.CommandText = "CREATE TABLE ATM(id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, balance INT);";
            _command.ExecuteNonQuery();
        }

        Close();
    }

    static List<string> ReadFile()
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
        if(rowsInFile.Contains(1))
        {
            return true;
        }
    }


    private void CheckSQLiteVersion()
    {
        Open();
        string stm = "SELECT SQLITE_VERSION();";
        _command = new SQLiteCommand(stm, _connection);
        string? version = _command.ExecuteScalar().ToString();

        Close();
    }

    private void GetCurrentTime()
    {
        Open();
        string stm = "SELECT(datetime('now', 'localtime'));";
        _command = new SQLiteCommand(stm, _connection);
        string? date = _command.ExecuteScalar().ToString();
        Close();
    }
}