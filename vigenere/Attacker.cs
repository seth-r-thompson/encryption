using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace AttackLibrary
{
    public abstract class Attacker : BackgroundWorker
    {
        private string ciphertextFile, key;

        public Attacker(string ciphertextFile) : base()
        {
            this.ciphertextFile = ciphertextFile;

            this.WorkerReportsProgress = true;
            this.DoWork += AttackerDoWork;
        }

        protected void AttackerDoWork(object sender, DoWorkEventArgs e)
        {
            var data = new Dictionary<char, double>();
            double count = 0;

            using (StreamReader reader = new StreamReader(ciphertextFile))
            {
                // Calculate occurence of each character
                do
                {
                    char c = (char)reader.Read();

                    if (data.ContainsKey(c))
                    {
                        data[c]++;
                    }
                    else
                    {
                        data.Add(c, 1);
                    }

                    count++;
                } while (!reader.EndOfStream);

                // Divide each character to calculate probability in set
                foreach (var kvp in data)
                {
                    data[kvp.Key] = data[kvp.Key] / count;
                }

                // Calculate IOC
                double IOC = 0;
                int progress = 0;
                
                foreach (var kvp in data)
                {
                    IOC += kvp.Value * kvp.Value;
                    (sender as BackgroundWorker).ReportProgress((int)(count - progress++), IOC);
                }

                e.Result = IOC;
            }
        }
    }
}
