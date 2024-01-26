using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DobotClientDemo
{
    public class Node<T>
    {
        public T _data;
        public Node<T> _next;
    
        public Node(T data)
        {
            _data = data;
            _next = null;
        }
    }

    public class EncryptionList<T>
    {
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


        private void Encrypt()
        {
            List<string> key = ReadFile();
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

        private void Decrypt()
        {

        }

    }

}
