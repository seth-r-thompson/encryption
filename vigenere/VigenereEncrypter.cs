using System;
using System.Collections.Generic;
using System.Text;

namespace EncryptionLibrary
{
    public class VigenereEncrypter : Encrypter
    {
        private const int LANGUAGE_SIZE = 256;

        public VigenereEncrypter(string plaintextFile, string key) : base(plaintextFile, key)
        {
            if (key.ToLower() != key)
            {
                throw new Exception();
            }
        }

        protected override char Encrypt(char c, string key, int keyIndex)
        {
            return (char)((c + key[keyIndex]) % LANGUAGE_SIZE);
        }
    }
}