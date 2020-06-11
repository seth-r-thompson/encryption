using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EncryptionLibrary
{
    public abstract class Encrypter : BackgroundWorker
    {
        private string plaintextFile, ciphertextFile, key;

        private int filesize = 0;

        public Encrypter(string plaintextFile, string key) : base()
        {
            this.key = key;
            this.plaintextFile = plaintextFile;

            // Generate name of ciphertext file
            string ciphertextFileName = string.Concat("encrypted_", this.plaintextFile.Split('\\').Last());
            this.ciphertextFile = string.Concat(plaintextFile.Substring(0, plaintextFile.Length - plaintextFile.Split('\\').Last().Length), ciphertextFileName);

            // Calculate file size
            using (StreamReader reader = new StreamReader(plaintextFile))
            {
                do
                {
                    reader.Read();
                    filesize++;
                } while (!reader.EndOfStream);
            }

            this.WorkerReportsProgress = true;
            this.DoWork += EncrypterDoWork;
        }

        protected void EncrypterDoWork(object sender, DoWorkEventArgs e)
        {
            using (StreamReader reader = new StreamReader(plaintextFile))
            {
                int index = 0, count = 0;

                do
                {
                    char unencryptedChar = (char)reader.Read();
                    char encryptedChar = Encrypt(unencryptedChar, key, index > key.Length - 1 ? index = 0 : index++);

                    // Without sleep, calculation is too quick so it won't be displayed
                    Thread.Sleep(1);

                    // Calculate and report progress
                    var progress = (100 * count++ / filesize);
                    (sender as BackgroundWorker).ReportProgress(progress, encryptedChar);

                } while (!reader.EndOfStream);
            }

            e.Result = ciphertextFile;
        }

        protected abstract char Encrypt(char c, string key, int keyIndex);
    }
}