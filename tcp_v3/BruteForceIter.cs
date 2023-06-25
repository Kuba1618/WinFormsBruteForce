using System;
using System.Collections;
using System.IO;
using System.Text;

namespace tcp_v3
{
    public class BruteForceIter : IEnumerable
    {
        #region constructors 
        private StringBuilder sb = new StringBuilder();

        //the string we want to permutate 
        public string alphabet = "!#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        private ulong len;
        private int _max;
        public int max { get { return _max; } set { _max = value; } }
        private int _min;
        public int min { get { return _min; } set { _min = value; } }
        #endregion

        #region Methods
        public System.Collections.IEnumerator GetEnumerator()
        {
            len = (ulong)this.alphabet.Length;
            for (double x = min; x <= max; x++)
            {
                ulong total = (ulong)Math.Pow((double)alphabet.Length, (double)x);
                ulong counter = 0;
                while (counter < total)
                {
                    string a = factoradic(counter, x - 1);
                    yield return a;
                    counter++;
                }
            }
        }
        private string factoradic(ulong l, double power)
        {
            sb.Length = 0;
            while (power >= 0)
            {
                sb = sb.Append(this.alphabet[(int)(l % len)]);
                l /= len;
                power--;
            }
            return sb.ToString();
        }

        public void SaveResultToFile(string text)
        {
            string path1 = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString() + "\\brokenPasswords" + ".txt";

            if (!File.Exists(path1))
            {
                using (StreamWriter sw = File.CreateText(path1))
                {
                    sw.WriteLine(text);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path1))
                {
                    sw.WriteLine(text);
                }
            }
        }
        #endregion
    }
}
