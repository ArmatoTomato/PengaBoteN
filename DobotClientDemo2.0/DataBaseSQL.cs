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
}
    //Skapa en fil och kolla om den finns om ej 