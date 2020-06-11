using System;
using System.Collections.Generic;
using System.Text;

namespace DecryptionLibrary
{
    public class VigenereDecrypter : Decrypter
    {
        private const int LANGUAGE_SIZE = 256;

        public VigenereDecrypter(string ciphertextFile, string key) : base(ciphertextFile, key)
        {
            if (key.ToLower() != key)
            {
                throw new Exception();
            }
        }

        protected override char Decrypt(char c, string key, int keyIndex)
        {
            return (char)((c - key[keyIndex]) % LANGUAGE_SIZE);
        }
    }
}
