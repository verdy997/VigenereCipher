using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigenereCipher
{
    public class Cipher
    {
        static List<char> alphabet = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        static List<double> prob_EN = new List<double> 
        { 0.0657, 0.0126, 0.0399, 0.0322, 0.0957, 0.0175, 0.0145, 0.0404, 0.0701, 0.0012, 0.0049, 0.0246, 0.0231, 0.0551, 0.0603, 0.0298, 0.0005, 0.0576, 0.0581, 0.0842, 0.0192, 0.0081, 0.0086, 0.0007, 0.0167, 0.0005};

        static List<double> prob_SK = new List<double>
        { 0.0995, 0.0118, 0.0266, 0.0436, 0.0698, 0.0113, 0.0017, 0.0175, 0.0711, 0.0157, 0.0406, 0.0262, 0.0354, 0.0646, 0.0812, 0.0179, 0.0000, 0.0428, 0.0463, 0.0432, 0.0384, 0.0314, 0.0000, 0.0004, 0.0170, 0.0175};

        static string path = @"..\texts\text4_enc.txt";
        string contents = File.ReadAllText(path);
        string original = string.Empty;
        int min = 15;
        int max = 25;


        public Cipher()
        {
            original = contents;
            contents = RemoveWhiteSpaces(contents);
            List<int> diffrences = FindTreeLetters(contents);
            List<int> devisors = FindDevisor(diffrences, min, max);
            List<string> splited = SplitMessage(contents, MostUsedValue(devisors));
            List<string> messy = new List<string>();

            foreach (var w in splited)
            {
                messy.Add(Decipher(w, true, GetProbabilities(w)));
            }

            Repair(messy);
        }

        private string RemoveWhiteSpaces(string input)
        {
            return new string(input.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }

        private List<int> FindTreeLetters(string input)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < input.Length - 3; i++)
            {
                for (int j = i + 1; j  < input.Length - 3; j++)
                {
                    if (input[i] == input[j] && input[i + 1] == input[j + 1] && input[i + 2] == input[j + 2])
                    {
                        string s = $"{input[i]}{input[i+1]}{input[i+2]}";
                        //Console.WriteLine($"{j-i}, ({s})\n");
                        result.Add(j-i);
                    }
                }
            }

            return result;
        }

        private List<int> FindDevisor(List<int> diffs, int lower, int upper)
        {
            List<int> devisors = new List<int>();

            foreach (int diff in diffs)
            {
                for (int i = lower; i <= upper; i++)
                {
                    if ((diff%i) == 0)
                    {
                        devisors.Add(i);
                    }
                }
            }

            return devisors;
        }

        private int MostUsedValue(List<int> devs)
        {
            int maxcount = 0;
            int maxFreq = 0;

            for (int i = 0; i < devs.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < devs.Count; j++)
                {
                    if (devs[i] == devs[j])
                    {
                        count++;
                    }
                }

                if (count > maxcount)
                {
                    maxcount = count;
                    maxFreq = devs[i];
                }
            }
            Console.WriteLine($"Key is long: {maxFreq}");
            return maxFreq;
        }

        private List<string> SplitMessage(string msg, int dev)
        {
            List<string> splited = new List<string>();

            for (int i = 0; i < dev; i++)
            {
                string s = "";
                for (int j = i; j < msg.Length; j+=dev)
                {
                    s = string.Concat(s, msg[j]);
                }
                splited.Add(s);
            }

            return splited;
        }

        private List<LetterProb> GetProbabilities(string substring)
        {
            string source = substring;
            int countAll = substring.Length;
            int countUnique = substring.Distinct().Count(); // pocet unikatnych charakterov
            List<LetterProb> probabilities = new List<LetterProb>();

            foreach (var letter in alphabet)
            {
                probabilities.Add(new LetterProb { Letter = letter, Probability = 0 });
            }

            for (int i = 0; i <= countUnique - 1; i++)
            {
                int countF = substring.Count(l => l == source[0]);
                var prob = probabilities.SingleOrDefault(x => x.Letter == source[0]);

                if (prob != null)
                {
                    double p = ((double)countF/(double)countAll);
                    prob.Probability = p;
                }

                source = source.Replace(source[0].ToString(), string.Empty);
            }

            return probabilities;
        }

        private string Decipher(string word, bool slovak, List<LetterProb> letterProbs)
        {
            string deciphered = string.Empty;
            byte[] asciiBytes = Encoding.ASCII.GetBytes(word);
            List<int> asciiINT = new List<int>();
            

            for (int i = 0; i <= asciiBytes.Length -1; i++)
            {
                asciiINT.Add(((int)asciiBytes[i]) - 65);
            }

            if (slovak)
            {
                deciphered = GetSlovakString(letterProbs, asciiINT);
            }

            return deciphered;
        }

        private string GetSlovakString(List<LetterProb> letterProbs, List<int> asciiINT)
        {
            string word = string.Empty;
            double prob = 999999;
            int bestShift = 0;

            for (int i = 0; i < prob_SK.Count; i++)
            {
                double helper = 0;
                int shift = 0;
                foreach (var item in letterProbs)
                {
                    if (shift + i >= prob_SK.Count)
                    {
                        helper += Math.Abs(prob_SK[(shift + i)-prob_SK.Count] - item.Probability);
                    } else
                    {
                        helper += Math.Abs(prob_SK[shift + i] - item.Probability);
                    }
                    shift++;

                }

                if (helper < prob)
                {
                    prob = helper;
                    bestShift = i;
                }
            }

            int maxIndex = prob_SK.ToList().IndexOf(prob_SK.Max());
            List<int> pom = new List<int>();

            foreach (var letter in asciiINT)
            {
                pom.Add(((letter + bestShift)%26)+65);
            }

            foreach (var item in pom)
            {
                word = string.Concat(word, ((char)(item)).ToString());
            }

            return word;
        }

        private string Repair(List<string> mess)
        {
            string decodeWord = string.Empty;

            for (int i = 0; i < mess[0].Length; i++)
            {
                foreach (var word in mess)
                {
                    if (i >= word.Length)
                    {
                        continue;
                    }
                    decodeWord = string.Concat(decodeWord, word[i]);
                }
            }

            Console.WriteLine("--------------------------");
            Console.WriteLine("MESSAGE IS:");

            List<int> WhiteSpacesIndexes = new List<int>();

            for (int i = 0; i < original.Length; i++)
            {
                if (Char.IsWhiteSpace(original[i]))
                {
                    WhiteSpacesIndexes.Add(i);
                }
            }

            for (int i = 0; i < WhiteSpacesIndexes.Count; i++)
            {
                decodeWord = decodeWord.Insert(WhiteSpacesIndexes[i], " ");
            }

            Console.WriteLine(decodeWord);
            return decodeWord;
            
        }
    }


}
