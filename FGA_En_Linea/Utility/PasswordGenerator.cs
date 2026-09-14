using System;
using System.Text;
using System.Security.Cryptography;

namespace FGA.Utility
{

    public sealed class ControlChars
    {
        public const char Back = '\b';
        public const char Cr = '\r';
        public const string CrLf = "\r\n";
        public const char FormFeed = '\f';
        public const char Lf = '\n';
        public const string NewLine = "\r\n";
        public const char NullChar = '\0';
        public const char Quote = '"';
        public const char Tab = '\t';
        public const char VerticalTab = '\v';
    }
    public class PasswordGenerator
    {
        public PasswordGenerator()
        {
            this.Minimum = DefaultMinimum;
            this.Maximum = DefaultMaximum;
            this.ConsecutiveCharacters = false;
            this.RepeatCharacters = true;
            this.ExcludeSymbols = true;
            this.Exclusions = null;

            rng = new RNGCryptoServiceProvider();
        }

        protected int GetCryptographicRandomNumber(int lBound, int uBound)
        {
            // Assumes lBound >= 0 && lBound < uBound
            // returns an int >= lBound and < uBound
            uint urndnum;
            byte[] rndnum = new Byte[4];
            if (lBound == uBound - 1)
                // test for degenerate case where only lBound can be returned
                return lBound;

            uint xcludeRndBase = (uint.MaxValue - (uint.MaxValue % System.Convert.ToUInt32(uBound - lBound)));

            do
            {
                rng.GetBytes(rndnum);
                urndnum = System.BitConverter.ToUInt32(rndnum, 0);
            }
            while (urndnum >= xcludeRndBase);

            return System.Convert.ToInt32(urndnum % (uBound - lBound)) + lBound;
        }

        protected char GetRandomCharacter()
        {
            int upperBound = pwdCharArray.GetUpperBound(0);

            if (true == this.ExcludeSymbols)
                upperBound = PasswordGenerator.UBoundDigit;

            int randomCharPosition = GetCryptographicRandomNumber(pwdCharArray.GetLowerBound(0), upperBound);

            char randomChar = pwdCharArray[randomCharPosition];

            return randomChar;
        }

        public string Generate()
        {
            // Pick random length between minimum and maximum   
            int pwdLength = GetCryptographicRandomNumber(this.Minimum, this.Maximum);

            StringBuilder pwdBuffer = new StringBuilder();
            pwdBuffer.Capacity = this.Maximum;
            char lastCharacter;

            // Generate random characters
            char nextCharacter = 'A';

            // Initial dummy character flag
            lastCharacter = InlineAssignHelper(ref nextCharacter, ControlChars.Lf);

            for (int i = 0; i <= pwdLength - 1; i++)
            {
                nextCharacter = GetRandomCharacter();

                if (false == this.ConsecutiveCharacters)
                {
                    while (lastCharacter == nextCharacter)
                        nextCharacter = GetRandomCharacter();
                }

                if (false == this.RepeatCharacters)
                {
                    string temp = pwdBuffer.ToString();
                    int duplicateIndex = temp.IndexOf(nextCharacter);
                    while (-1 != duplicateIndex)
                    {
                        nextCharacter = GetRandomCharacter();
                        duplicateIndex = temp.IndexOf(nextCharacter);
                    }
                }

                if ((this.Exclusions != null))
                {
                    while (-1 != this.Exclusions.IndexOf(nextCharacter))
                        nextCharacter = GetRandomCharacter();
                }

                pwdBuffer.Append(nextCharacter);
                lastCharacter = nextCharacter;
            }

            // Se declaran las variables necesarias para la encripcion.
            Encripcion.Encripcion enc = new Encripcion.Encripcion();
            MD5CryptoServiceProvider Md5 = new MD5CryptoServiceProvider();
            UnicodeEncoding Ue = new UnicodeEncoding();

            if (pwdBuffer != null)

                // Dim ByteSourceText() As Byte = Ue.GetBytes(pwdBuffer.ToString())
                // Return Convert.ToBase64String(Md5.ComputeHash(ByteSourceText))
                return enc.EncryptStr(pwdBuffer.ToString());
            else
                return String.Empty;
        }

        public string Exclusions
        {
            get
            {
                return this.exclusionSet;
            }
            set
            {
                this.exclusionSet = value;
            }
        }

        public int Minimum
        {
            get
            {
                return this.minSize;
            }
            set
            {
                this.minSize = value;
                if (PasswordGenerator.DefaultMinimum > this.minSize)
                    this.minSize = PasswordGenerator.DefaultMinimum;
            }
        }

        public int Maximum
        {
            get
            {
                return this.maxSize;
            }
            set
            {
                this.maxSize = value;
                if (this.minSize >= this.maxSize)
                    this.maxSize = PasswordGenerator.DefaultMaximum;
            }
        }

        public bool ExcludeSymbols
        {
            get
            {
                return this.hasSymbols;
            }
            set
            {
                this.hasSymbols = value;
            }
        }

        public bool RepeatCharacters
        {
            get
            {
                return this.hasRepeating;
            }
            set
            {
                this.hasRepeating = value;
            }
        }

        public bool ConsecutiveCharacters
        {
            get
            {
                return this.hasConsecutive;
            }
            set
            {
                this.hasConsecutive = value;
            }
        }

        private const int DefaultMinimum = 6;
        private const int DefaultMaximum = 10;
        private const int UBoundDigit = 57;

        private RNGCryptoServiceProvider rng;
        private int minSize;
        private int maxSize;
        private bool hasRepeating;
        private bool hasConsecutive;
        private bool hasSymbols;
        private string exclusionSet;
        private char[] pwdCharArray =( "abcdefghjkmnopqrstuvwxyzABCDEFG" + "HJKMNOPQRSTUVWXYZ0123456789@#$%&*").ToCharArray();

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }
    }
}