using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace ContentScramblingSystem
{
    class CSS
    {
        #region CONSTANTS

        const int KEYSIZE = 40;
        const int SHIFTCOUNT = 8;

        #endregion

        #region STRING HELPERS

        private static bool TrySanitize(string s, out string? r)
        {
            s.Trim();

            foreach (char c in s)
            {
                if (c != '0' && c != '1')
                {
                    Console.WriteLine("String wasn't in bits");
                    r = null;
                    return false;
                }
            }

            r = s;
            return true;
        }

        private static string[] Tokenize(string s, int count)
        {
            string[] tokens = new string[count];

            for (int i = 0; i < count; i++)
            {
                tokens[i] = s.Substring(i * (s.Length / count), s.Length / count);
            }

            return tokens;
        }

        #endregion

        #region BIT OPERATION HELPERS

        private static char TwoBitXOR(char c1, char c2)
        {
            if (c1 == c2) return '0';
            else return '1';
        }

        private static char FourBitXOR(char c1, char c2, char c3, char c4)
        {
            return TwoBitXOR(TwoBitXOR(c1, c2), TwoBitXOR(c3, c4));
        }

        private static bool TryFullAdd(string s1, string s2, out string? r, out char? carryOut, char carryIn = '0')
        {
            r = string.Empty;
            carryOut = carryIn;

            if (s1.Length != s2.Length)
            {
                Console.WriteLine("Can't full add things of different size");
                r = null;
                carryOut = null;
                return false;
            }
            
            for (int i = s1.Length - 1; i >= 0; i--)
            {
                int c1 = s1[i] == '0' ? 0 : 1;
                int c2 = s2[i] == '0' ? 0 : 1;

                var bit = c1 + c2;

                if (carryOut == '1') bit++;

                switch (bit)
                {
                    case 0:
                        r = "0" + r;
                        carryOut = '0';
                        break;
                    case 1:
                        r = "1" + r;
                        carryOut = '0';
                        break;
                    case 2:
                        r = "0" + r;
                        carryOut = '1';
                        break;
                    case 3:
                        r = "1" + r;
                        carryOut = '1';
                        break;
                    default:
                        r = null;
                        carryOut = null;
                        return false;
                }
            }
            
            return true;
        }

        private static string Shift(string s, char c)
        {
            return c + s.Substring(0, s.Length - 1);
        }

        #endregion

        private static bool TryGenerateKey(string seed, out string? key)
        {
            if (TrySanitize(seed, out seed) == false || seed.Length != KEYSIZE)
            {
                Console.WriteLine("Couldn't generate key...");
                key = null;
                return false;
            }

            // Tokenize seed into 5 bytes
            var tokens = Tokenize(seed, 5);

            // Initialize Shift Register 1 (17 bit register)
            var sr1 = tokens[0] + tokens[1].Substring(0, 5) + "1" + tokens[1].Substring(5, 3); // 4th bit seeded as 1
            // Initialize Shift Register 2 (25 bit register)
            var sr2 = tokens[2] + tokens[3] + tokens[4].Substring(0, 5) + "1" + tokens[4].Substring(5, 3); // 4th bit seeded as 1

            // Perform & store shifts
            for (int i = 0; i < SHIFTCOUNT; i++)
            {
                // XOR tapped bits of SR1 (15th & 1st)
                var bit = TwoBitXOR(sr1[2], sr1[16]);

                // Shift SR1
                Shift(sr1, bit);

                // XOR tapped bits of SR2 (15th & 5th & 4th & 1st)
                bit = FourBitXOR(sr2[10], sr2[20], sr2[21], sr2[24]);

                // Shift SR2
                Shift(sr2, bit);
            }

            // Get most significant byte of SR1 and SR2
            var byteSR1 = sr1.Substring(0, 8);
            var byteSR2 = sr2.Substring(0, 8);            

            if (TryFullAdd(byteSR1, byteSR2, out key, out char? carry) == false)
            {
                Console.WriteLine("Full add failed");
                key = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void Cipher(string file, bool encryptMode = true)
        {
            string textOutput = string.Empty;

            // Get seed
            Console.WriteLine("\nEnter the {0} key", encryptMode ? "encryption" : "decryption");
            var seed = Console.ReadLine();

            // Open and read file
            string textInput;
            try {
                using (StreamReader reader = File.OpenText(file))
                {
                    textInput = reader.ReadToEnd().Trim();
                    if (TrySanitize(textInput, out textInput) == false)
                    {
                        Console.WriteLine("File couldn't be parsed");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("\nInput:\t{0}\n", textInput);

            // Tokenize file into bytes
            var tokens = Tokenize(textInput, textInput.Length / 8);
            
            // Encrypt file
            for (int i = 0; i < tokens.Length; i++)
            {
                // Generate key
                if (TryGenerateKey(seed, out var key) == false)
                {
                    Console.WriteLine("Encryption failed");
                    return;
                }

                //Console.WriteLine("Segment {0}: {1} BXOR {2}:", i, tokens[i], key); 

                // Bitwise XOR of keystream & token
                for (int j = 0; j < tokens[i].Length; j++)
                {
                    var bit = TwoBitXOR(tokens[i][j], key[j]);
                    textOutput = textOutput + bit;
                    //Console.WriteLine("\t{0} XOR {1} = {2}", tokens[i][j], key[j], bit);
                }

                // Pad key back onto seed for later use
                seed = key + seed.Substring(8);
            }

            Console.WriteLine("Output:\t{0}\n", textOutput);

            // Create and write output file
            using (StreamWriter writer = File.CreateText(file.Split('.')[0] + (encryptMode ? "_encrypted.txt" : "_decrypted.txt")))
            {
                writer.WriteLine(textOutput);
            }
        }

        static void Main(string[] args)
        {
            bool loop = true;

            do
            {
                Console.WriteLine("\n(E)ncrypt or (D)ecrypt or e(X)it");

                switch (Console.ReadLine().Trim()[0])
                {
                    case 'e':
                    case 'E':
                        Console.WriteLine("\nEnter file to encrypt...");
                        Cipher(Console.ReadLine());
                        // goto case 'x';
                        break;
                    case 'd':
                    case 'D':
                        Console.WriteLine("\nEnter file to decrypt...");
                        Cipher(Console.ReadLine(), false);
                        // goto case 'x';
                        break;
                    case 'x':
                    case 'X':
                        Console.WriteLine("\nQuitting...");
                        loop = false;
                        break;
                    default:
                        Console.WriteLine("\nPlease enter a valid operation!");
                        break;
                }
            } while (loop);
        }
    }
}