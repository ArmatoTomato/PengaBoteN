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
                StreamReader sr = new StreamReader("Encryption.txt");

                line = sr.ReadLine();

                while (line != null)
                {
                    AddLast((T)(Convert.ChangeType(line, typeof(T))));
                    line = sr.ReadLine();
                    rowsInFile.Add(line);
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

            string smallPassword = password.ToLower();

            int i = 0;
            char[] test = smallPassword.ToCharArray();
            int s = test.GetLength(0);

            while (i != s)
            {
                if (temp._data.ToString() == test[i].ToString())
                {
                    PasswordList.Add(temp._next._data.ToString());
                    i++;
                    temp = _first;
                }
                temp = temp._next;
            }

            return PasswordList;
        }

        public string Decrypt(int id)
        {
            db = new DataBaseSQL("ATM.db");

            string password = db.GetPassword(id);
            List<string> DecryptedList = new List<string>();
            char[] test = password.ToCharArray();
            int s = test.GetLength(0);

            ReadFile();
            Node<T> temp = _first;

            //for (int ia = 0; ia < test.GetLength(0);)
            //{
            //    temp = AddLast((T)(Convert.ChangeType(test[ia], typeof(T))));
            //    ia++;
            //}

            int i = 0;

            while (i != s)
            {
                if (temp._data.ToString() == test[i].ToString())
                {
                    DecryptedList.Add(temp._prev._data.ToString());
                    i++;
                    temp = _first;
                }
                temp = temp._next;
            }

            DecryptedList.ToString().Trim();
            string decryptedPassword = string.Join("", DecryptedList.ToArray());

            return decryptedPassword;
        }
    }
}
