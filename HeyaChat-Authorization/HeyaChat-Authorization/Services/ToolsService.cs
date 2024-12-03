using HeyaChat_Authorization.Services.Interfaces;

namespace HeyaChat_Authorization.Services
{
    public class ToolsService : IToolsService
    {
        public ToolsService()
        {

        }

        public string MaskPhoneNumber(string phoneNumber)
        {
            string firstSect = phoneNumber.Substring(0, 4);
            string lastSect = phoneNumber.Substring(phoneNumber.Length - 1, 1);
            string middleSect = new string('*', phoneNumber.Length - 5);

            return firstSect + middleSect + lastSect;
        }

        public string MaskEmail(string email)
        {
            // Get first letters
            string firstLetter = email.Substring(0, 1);
            string secondLetter = email.Substring(email.IndexOf('.') + 1, 1);
            int secondLetterIndex = 0;

            if (email.IndexOf('.') < email.IndexOf('@'))
            {
                secondLetterIndex = email.IndexOf('.') + 1;
            }

            string maskedEmail = firstLetter;

            // Replace all characters except . and @ with *
            for (int i = 1; i < email.Length; i++)
            {
                char letter = email[i];

                if (letter != '.' && letter != '@' && i != secondLetterIndex)
                {
                    maskedEmail += '*';
                }
                else if (i == secondLetterIndex)
                {
                    maskedEmail += secondLetter;
                    i++;
                }
                else 
                {
                    maskedEmail += letter;
                }
            }

            return maskedEmail;
        }


    }
}
