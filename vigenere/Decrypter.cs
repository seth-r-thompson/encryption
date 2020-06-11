using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DecryptionLibrary
{
    public abstract class Decrypter : BackgroundWorker
    {
        private string plaintextFile, ciphertextFile, key;

        private int filesize = 0;

        public Decrypter(string ciphertextFile, string key) : base()
        {
            this.key = key;
            this.ciphertextFile = ciphertextFile;

            // Generate name of plaintext file
            string plaintextFileName  = string.Concat("decrypted_", this.ciphertextFile.Split('\\').Last());
            this.plaintextFile = string.Concat(ciphertextFile.Substring(0, ciphertextFile.Length - ciphertextFile.Split('\\').Last().Length), plaintextFileName);

            // Calculate file size
            using (StreamReader reader = new StreamReader(ciphertextFile))
            {
                do
                {
                    reader.Read();
                    filesize++;
                } while (!reader.EndOfStream);
            }

            this.WorkerReportsProgress = true;
            this.DoWork += DecrypterDoWork;
        }

        protected void DecrypterDoWork(object sender, DoWorkEventArgs e)
        {
            using (StreamReader reader = new StreamReader(ciphertextFile))
            {
                int index = 0, count = 0;

                do
                {
                    char encyptedChar = (char)reader.Read();
                    char unencryptedChar = Decrypt(encyptedChar, key, index > key.Length - 1 ? index = 0 : index++);

                    // Without sleep, calculation is too quick so it won't be displayed
                    Thread.Sleep(1);

                    // Calculate and report progress
                    var progress = (100 * count++ / filesize);
                    (sender as BackgroundWorker).ReportProgress(progress, unencryptedChar);

                } while (!reader.EndOfStream);
            }

            e.Result = plaintextFile;
        }

        protected abstract char Decrypt(char c, string key, int keyIndex);
    }
}
