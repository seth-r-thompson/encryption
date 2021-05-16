using System;

namespace ToyTetragraphHash
{
    class Program
    {
        /* CONVERTERS */
        private static int[] ToInts(string input, bool usePadding = true)
        {
            // Calculate padding needed
            int padding = input.Length % 16 != 0 ? 16 - (input.Length % 16) : 0;

            int[] output = usePadding ? new int[input.Length + padding] : new int[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = Convert.ToInt32(input[i]) - 65;
            }

            return output;
        }

        private static string ToChars(int[] input)
        {
            var output = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                output += Convert.ToChar(input[i] + 65);
            }

            return output;
        }

        private static string Clean(string input)
        {
            var output = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                if (Convert.ToInt32(input[i]) >= 65 && Convert.ToInt32(input[i]) <= 90) output += input[i];
            }

            return output;
        }

        /* HASH */
        private static string ToyTetragraphHash(string input)
        {
            // Initial values
            int[] plain = ToInts(input);
            int[] cipher = { 0, 0, 0, 0 };
            
            // Perform algorithm on each block
            for (int i = 0; i < plain.Length; i+=16)
            {
                // Total for current block round one
                int[] total = { 0, 0, 0, 0 };
                
                // Round one summands
                for (int j = 0, k = 0; j < 16; j++, k++)
                {
                    // Increase total
                    total[k] += plain[i + j];

                    // Reset k
                    if (k >= 3) k = -1;
                }

                // Add round one to cipher
                for (int l = 0; l < 4; l++)
                {
                    cipher[l] = ((total[l] % 26) + cipher[l]) % 26;
                }
                
                // Total for current block round two
                total = new int[]{ 0, 0, 0, 0 };
                
                // Round two summands
                for (int j = 0, k = 3; j < 16; j++)
                {
                    // Increase total
                    total[k] += plain[i + j];

                    // Reset k
                    if (k >= 3 && j <= 11) k = -1;

                    // Shift k based on row
                    if (j == 3 && k == 2) k = 1;
                    if (j == 7 && k == 1) k = 0;
                    if (j == 11 && k == 0) k = 2;

                    // Increment (or decrement on last row)
                    k += j <= 11 ? 1 : -1;
                }

                // Add round two to cipher
                for (int l = 0; l < 4; l++)
                {
                    cipher[l] = ((total[l] % 26) + cipher[l]) % 26;
                }
            }

            // Return hash
            return ToChars(cipher);
        }

        /* DRIVER */
        static void Main(string[] args)
        {
            while (true)
            {
                // Get input
                Console.Write("Enter string to hash:\t");
                var input = Clean(Console.ReadLine().Trim().ToUpper());

                // Display output
                Console.WriteLine("Calculating hash for {0}...", input);
                Console.WriteLine("Hash for input is:\t{0}", ToyTetragraphHash(input));

                // Exit prompt
                Console.Write("Exit? [Y/N]\t");
                if (Console.ReadLine().Trim().ToLower()[0] == 'y') break;

                Console.WriteLine();
            }
        }
    }
}
