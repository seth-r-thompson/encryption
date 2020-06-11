using AttackLibrary;
using DecryptionLibrary;
using EncryptionLibrary;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CryptoSystems
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFileDialog;
        private string currentFile;

        public MainWindow()
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            InitializeComponent();
        }

        private void OpenPlaintextButton_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                this.ClearText();

                currentFile = openFileDialog.FileName;

                PlaintextFileTextBlock.Text = openFileDialog.FileName.Split('\\').Last();
                PlaintextTextBlock.Text = File.ReadAllText(openFileDialog.FileName);
            }

            Mouse.OverrideCursor = null;
        }

        private void OpenCiphertextButton_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                this.ClearText();

                currentFile = openFileDialog.FileName;

                CiphertextFileTextBlock.Text = openFileDialog.FileName.Split('\\').Last();
                CiphertextTextBlock.Text = File.ReadAllText(openFileDialog.FileName);
            }

            Mouse.OverrideCursor = null;
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            Encrypter encrypter;
            
            this.ClearText();

            if (VigenereRadioButton.IsChecked == true)
            {
                if (VigenereInputKey.Text != VigenereInputKey.Text.ToLower())
                {
                    this.Error("Choose a lowercase key.");
                }
                else if (VigenereInputKey.Text == string.Empty)
                {
                    this.Error("Enter a key.");
                }
                else
                {
                    encrypter = new VigenereEncrypter(currentFile, VigenereInputKey.Text);
                    encrypter.ProgressChanged += EncryptUpdateHandler;
                    encrypter.RunWorkerCompleted += EncryptCompletionHandler;
                    encrypter.RunWorkerAsync();
                }
            }
            else
            {
                this.Error("Select an encryption method.");
            }
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            Decrypter decrypter;

            this.ClearText();

            if (VigenereRadioButton.IsChecked == true)
            {
                if (VigenereInputKey.Text != VigenereInputKey.Text.ToLower())
                {
                    this.Error("Choose a lowercase key.");
                }
                else if (VigenereInputKey.Text == string.Empty)
                {
                    this.Error("Enter a key.");
                }
                else
                {
                    decrypter = new VigenereDecrypter(currentFile, VigenereInputKey.Text);
                    decrypter.ProgressChanged += DecryptUpdateHandler;
                    decrypter.RunWorkerCompleted += DecryptCompletionHandler;
                    decrypter.RunWorkerAsync();
                }
            }
            else
            {
                this.Error("Select a decryption method.");
            }
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            Attacker attacker;

            if (VigenereRadioButton.IsChecked == true)
            {
                // attacker = new VigenereAttacker(currentFile);
                // attacker.RunWorkerAsync();
            }
            else
            {
                this.Error("Select an attack method.");
            }

        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFile != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var data = new Dictionary<char, int>();

                // Calculate occurence of each character
                using (StreamReader reader = new StreamReader(currentFile))
                {
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

                    } while (!reader.EndOfStream);
                }

                // Generate filename
                string statisticsFile = string.Concat(currentFile.Split('/').Last().Split('.').First(), "_statistics.csv");

                // Write statistics file
                foreach (var kvp in data)
                {
                    string key = string.Empty;

                    // Cleanup symbols that are problematic for CSV
                    if (kvp.Key == '\n') { key = "linebreak"; }
                    if (kvp.Key == ',') { key = "comma"; }
                    if (kvp.Key == '"') { key = "quote";  }
                    if (kvp.Key == ' ') { key = "space"; }
                    
                    File.AppendAllText(statisticsFile, string.Concat(key == string.Empty ? kvp.Key.ToString() : key, ",", (int)kvp.Key, ",", kvp.Value, "\n"));
                }

                Mouse.OverrideCursor = null;

                // Alert user when done
                MessageBox.Show("Statistics file generated.", "Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #region Async Methods

        private void EncryptUpdateHandler(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                EncryptButton.Visibility = Visibility.Hidden;
                EncryptProgress.Visibility = Visibility.Visible;

                EncryptProgress.Value = e.ProgressPercentage;
                CiphertextTextBlock.Text = string.Concat(CiphertextTextBlock.Text, (char)e.UserState);
            }
        }

        private void EncryptCompletionHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                EncryptButton.Visibility = Visibility.Visible;
                EncryptProgress.Visibility = Visibility.Hidden;

                CiphertextFileTextBlock.Text = ((string)e.Result).Split('\\').Last();

                // Update current file
                currentFile = (string)e.Result;

                // Write result to file
                File.WriteAllText((string)e.Result, CiphertextTextBlock.Text);
            }
        }

        private void DecryptUpdateHandler(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                DecryptButton.Visibility = Visibility.Hidden;
                DecryptProgress.Visibility = Visibility.Visible;

                DecryptProgress.Value = e.ProgressPercentage;
                PlaintextTextBlock.Text = string.Concat(PlaintextTextBlock.Text, (char)e.UserState);
            }
        }

        private void DecryptCompletionHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                DecryptButton.Visibility = Visibility.Visible;
                DecryptProgress.Visibility = Visibility.Hidden;

                PlaintextFileTextBlock.Text = ((string)e.Result).Split('\\').Last();

                // Update current file
                currentFile = (string)e.Result;

                // Write result to file
                File.WriteAllText((string)e.Result, PlaintextTextBlock.Text);
            }
        }

        #endregion

        #region Helper Methods

        private void ClearText()
        {
            PlaintextFileTextBlock.Text = string.Empty;
            PlaintextTextBlock.Text = string.Empty;

            CiphertextFileTextBlock.Text = string.Empty;
            CiphertextTextBlock.Text = string.Empty;
        }

        private void Error(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}