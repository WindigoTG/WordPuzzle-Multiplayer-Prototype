using System;

namespace WordPuzzle
{
    public class TextFormatter
    {
        public static string GetFormattedName(string firstName, string lastName) => $"{lastName[0]}. {firstName}";

        public static string GetFormattedName(string firstName, string lastName, string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                return GetFormattedName(firstName, lastName);

            return $"{lastName[0]}. {firstName} ({nickname})"; 
        }

        public static string GetFormattedNameTwoLines(string firstName, string lastName, string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                return GetFormattedName(firstName, lastName);

            return $"{lastName[0]}. {firstName}\n({nickname})";
        }

        public static string GetFormattedAge(string age)
        {
            if (int.TryParse(age, out int _))
            {
                string suffix = "лет";

                if (age.EndsWith("1"))
                    suffix = "год";

                if (age.EndsWith("2") || age.EndsWith("3") || age.EndsWith("4"))
                    suffix = "года";

                return $"{age} {suffix}";
            }
            else
                return "";
        }

        public static string GetFormattedAge(int age) => GetFormattedAge(age.ToString());

        public static string GetFormattedSolvedWordsAmount(int amount)
        {
            if (amount < 0)
                amount = 0;

            return $"Разгадано\nслов: <b>{amount}</b>";
        }

        public static string GetFormattedSolvedWordsAmount(string amount)
        {
            if (int.TryParse(amount, out int parsedAmount))
            {
                return GetFormattedSolvedWordsAmount(parsedAmount);
            }
            else
                return GetFormattedSolvedWordsAmount(0); ;
        }

        public static string GetFormattedTime(TimeSpan time)
        {
            return GetFormattedTime(time.TotalSeconds);
        }

        public static string GetFormattedTime(double totalSeconds)
        {
            return $"{(int)totalSeconds / 60}:{(int)totalSeconds % 60} мин.";
        }

        public static string GetFormattedTime(string totalSeconds)
        {
            if (double.TryParse(totalSeconds, out double seconds))
            {
                return GetFormattedTime(seconds);
            }
            else
                return GetFormattedTime(0);
        }

    }
}