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
        public static Node<T> _last = null;


        public static void AddLast(T data)
        {
            Node<T> newNode = new Node<T>(data);
            if (_first == null)
            {
                _first = newNode;
                _last = newNode;
            }
            else
            {
                _last._next = newNode;
                newNode._prev = _last;
                _last = newNode;
            }

            //Node<T> temp = _first;
            //while (temp._next != null)
            //{
            //    temp = temp._next;
            //}
            //temp._next = new Node<T>(data);

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
        public List<string> Encrypt(string password)
        {
            ReadFile();
            Node<T> temp = _first;
            List<string> PasswordList = new List<string>();

            char[] test = password.ToCharArray();

            int i = 0;

            while ((string)(Convert.ChangeType(temp, typeof(string))) != test[i].ToString())
            {
                if ((string)(Convert.ChangeType(temp, typeof(string))) == test[i].ToString())
                {
                    PasswordList.Add((string)(Convert.ChangeType(temp._next, typeof(string))));
                    i++;
                }
                temp = temp._next;
            }

            return PasswordList;
        }

        public void Decrypt(int id)
        {
            string password = db.GetPassword(id);
            List<string> DecryptedList = new List<string>();

            char[] test = password.ToCharArray();
            Node<T> temp = _first;

            int i = 0;

            while ((string)(Convert.ChangeType(temp, typeof(string))) != test[i].ToString())
            {
                if ((string)(Convert.ChangeType(temp, typeof(string))) != test[i].ToString())
                {
                    DecryptedList.Add((string)(Convert.ChangeType(temp._prev, typeof(string))));
                    i++;
                }
                temp = temp._next;
            }
        }
    }
}
