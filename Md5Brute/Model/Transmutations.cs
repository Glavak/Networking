using System;

namespace Model
{
    public static class Transmutations
    {
        public static char[] First(int length)
        {
            char[] prefix = new char[length];

            for (int i = 0; i < length; i++) prefix[i] = 'A';

            return prefix;
        }

        public static bool Next(char[] prefix)
        {
            for (int i = prefix.Length - 1; i >= 0; i--)
            {
                switch (prefix[i])
                {
                    case 'A':
                        prefix[i] = 'C';
                        return true;
                    case 'C':
                        prefix[i] = 'G';
                        return true;
                    case 'G':
                        prefix[i] = 'T';
                        return true;
                    case 'T':
                        prefix[i] = 'A';
                        continue;
                    default:
                        throw new Exception("Incorrect string");
                }
            }
            return false;
        }
    }
}
