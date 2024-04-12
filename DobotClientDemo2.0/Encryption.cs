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
    public class Node<T>//Generisk datatyp för att underlätta
    {
        public T _data;//Dubbellänkad lista som har relation till föregående och nästkommande nod
        public Node<T> _next;
        public Node<T> _prev;

        public Node(T data)
        {
            _data = data;
            _next = null;
            _prev = null;
        }
    }

    public class EncryptionList<T>//Hanterar dekryptionen
    {
        DataBaseSQL db;

        public static Node<T> _first = null;//Värdena börjar som inget
        public static Node<T> _last = null;

        public static void AddLast(T data)//Lägger till värdet sist
        {
            Node<T> newNode = new Node<T>(data);
            if (_first == null)//Om första är tomm läggs det till där och då blir det även sista 
            {
                _first = newNode;
                _last = newNode;
            }
            else//Finns det redan värde i listan läggs det nya till som sista och före detta sista blir näst sista
            {
                _last._next = newNode;
                newNode._prev = _last;
                _last = newNode;
            }
        }

        public static List<string> ReadFile()
        {
            List<string> rowsInFile = new List<string>();
            string line;

            try
            {
                StreamReader sr = new StreamReader("Encryption.txt");//Läser in krypteringsfilen
                line = sr.ReadLine();

                while (line != null)
                {
                    AddLast((T)(Convert.ChangeType(line, typeof(T))));//Lägger till det inlästa värdet sist i listan
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
            ReadFile();//Skapar rowsinfile listan
            Node<T> temp = _first;
            List<string> PasswordList = new List<string>();

            string smallPassword = password.ToLower();

            int i = 0;
            char[] passwordArray = smallPassword.ToCharArray();//Gör om lösenordet till en array
            int s = passwordArray.GetLength(0);//Array kan man hitta längden av

            while (i != s)//Körs så många tecken som lösenordet har
            {
                if (temp._data.ToString() == passwordArray[i].ToString())//Kollar om temp har samma värde som första tecknet i lösenordet
                {
                    PasswordList.Add(temp._next._data.ToString());//Isåfall läggs nästa värdet från inlästa filen till i lösenordslistan
                    i++;//Då ökar i
                    temp = _first;//Temp startar om, man kan ha samma tecken två gåner 
                }
                temp = temp._next;//Om värdena inte matchar går testas nästa värde från den inlästa krypteringsfilen tills rätt hittas
            }

            return PasswordList;//Returnerar krypterat lösenord
        }

        public string Decrypt(int id)//Omvänd kryptering, istället för att lägga till värdet ett senare tas det ett tidigare för att återställa lösenordet
        {
            db = new DataBaseSQL("ATM.db");

            string password = db.GetPassword(id);
            List<string> DecryptedList = new List<string>();
            char[] passwordArray = password.ToCharArray();
            int s = passwordArray.GetLength(0);

            ReadFile();
            Node<T> temp = _first;
            int i = 0;

            while (i != s)
            {
                if (temp._data.ToString() == passwordArray[i].ToString())
                {
                    DecryptedList.Add(temp._prev._data.ToString());
                    i++;
                    temp = _first;
                }
                temp = temp._next;
            }

            DecryptedList.ToString().Trim();//Tar bort mellanslag
            string decryptedPassword = string.Join("", DecryptedList.ToArray());//Gör om till string

            return decryptedPassword;//Krypterat och klart
        }
    }
}
