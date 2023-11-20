using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumaratorInterface
{
    public class User
    {
        // ===============================
        // AUTHOR      : Sinan KAPOĞLU
        // UPDATE DATE     : 20.08.2016
        // PURPOSE     : Class for User type. (Admin, Operator, Service.) Encrypt Alg. for hiding the real password
        // ===============================   
        public enum Users : int { Service = 0, Admin = 1, Operator = 2 };
        private Users user;
        public void setUserType(Users userType)
        {
            this.user = userType;
        }
        public int getUserType()
        {
            return (int)this.user;
        }
        public string UserName;
        public static string EncryptString(string stringToBeEncrypted, string Salt)
        {
            string Encrypted = "";
            int j = 0;
            for (int i = 0; i < stringToBeEncrypted.Length; ++i)
            {
                if (j == Salt.Length)
                    j = 0;
                char c = (char)((int)stringToBeEncrypted[i] ^ (int)Salt[j]);
                ++j;
                Encrypted = Encrypted + c;
            }
            return Encrypted;
        }
        public static string DecryptString(string stringToBeDecrypted, string Salt)
        {
            string Decrypted = "";
            int j = 0;
            for (int i = 0; i < stringToBeDecrypted.Length; ++i)
            {
                if (j == Salt.Length)
                    j = 0;
                char c = (char)((int)stringToBeDecrypted[i] ^ (int)Salt[j]);
                ++j;
                Decrypted = Decrypted + c;
            }
            return Decrypted;
        }
    }

}
