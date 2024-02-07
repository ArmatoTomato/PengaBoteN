using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace DobotClientDemo
{
    public class Node<T>
    {
        public T _data;
        public Node<T> _next;
        public Node<T> _prev;

        public Node(T data)
        {
            _data = data;
            _next = null;
            _prev = null;
        }
    }

    public class EncryptionList<T>
    {
        DataBaseSQL db;

        public static Node<T> _first = null;

        public static void AddLast(T data)
        {
            if (_first == null)
            {
                _first = new Node<T>(data);
            }

            Node<T> temp = _first;
            while (temp._next != null)
            {
                temp = temp._next;
            }
            temp._next = new Node<T>(data);
        }

        public static List<string> ReadFile()
        {
            List<string> rowsInFile = new List<string>();
            string line;
            try
            {
                StreamReader sr = new StreamReader("Encryption");

                line = sr.ReadLine();

                while (line != null)
                {
                    AddLast((T)(Convert.ChangeType(line, typeof(T))));
                    line = sr.ReadLine();
                }

                sr.Close();
            }
            catch (Exception error)
            {

            }
            finally
            {

            }

            return rowsInFile;
        }
        //public List<string> Encrypt(string x)
        //{
        //    Node<T> temp = _first;
        //    List<string> PasswordList = new List<string>();

        //    char[] test = x.ToCharArray();

        //    int i = 0;

        //    if ((string)(Convert.ChangeType(test, typeof(string)) != "")
        //    {
        //        while (temp._next != null)
        //        {
        //            if ((string)(Convert.ChangeType(temp, typeof(string))) == test[i])
        //            {
        //                PasswordList.Add((string)(Convert.ChangeType(temp._next, typeof(string))));
        //                i++;
        //            }
        //            temp = temp._next;
        //        }
        //    }
        //    return PasswordList;
        //}

        //private void Decrypt()
        //{
        //    List<string> PasswordList = db.GetPassword().Split(",").ToList();
        //    List<string> DecryptedList = new List<string>();
        //    Node<T> temp = _first;

        //    int i = 0;

        //    while (temp._next != null)
        //    {
        //        if ((string)(Convert.ChangeType(temp._next, typeof(string))) == PasswordList[i])
        //        {
        //            DecryptedList.Add((string)(Convert.ChangeType(temp, typeof(string))));
        //            i++;
        //        }
        //        temp = temp._next;
        //    }
        //}

        //Ladda in löseord från SQL
        //Gör om string till lista
        //Allt i små bokstäver
    }
}
