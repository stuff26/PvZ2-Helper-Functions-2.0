
using UniversalMethods;

namespace HelperFunctions
{
    public class ProgressChecker
    {
        public int CursorPosition { get; set; }
        public int ConsoleHeight { get; set; }
        public int MaxCount { get; set; }
        public int CurrentAmount { get; set; }
        public int NumErrors { get; set; }

        public ProgressChecker(string message, int maxCount)
        {
            CursorPosition = message.Length;
            ConsoleHeight = Console.GetCursorPosition().Top;
            MaxCount = maxCount;
            CurrentAmount = 0; NumErrors = 0;

            Console.ForegroundColor = ConsoleColor.Green;
            if (!message.EndsWith(' '))
            {
                message += " ";
                CursorPosition++;
            }
            UM.PrintColoredText(ConsoleColor.Green, message);
            Console.ForegroundColor = ConsoleColor.White;
            if (maxCount == 0)
            {
                UM.PrintColoredText(ConsoleColor.Yellow, $"{CurrentAmount}/{MaxCount}", separateLines:true);
            }
            else UM.PrintColoredText(ConsoleColor.White, $"{CurrentAmount}/{MaxCount}");
        }

        private void AdjustPosition()
        {
            Console.SetCursorPosition(CursorPosition, ConsoleHeight);
        }

        public void AddOne(bool adjustPosition = true)
        {
            CurrentAmount++;
            if (adjustPosition) AdjustPosition();
            if (CurrentAmount == MaxCount)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.White;
                
            Console.Write($"{CurrentAmount}/{MaxCount}");
            if (CurrentAmount == MaxCount)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
        }

        // UNUSED
        public void RemoveOne()
        {
            ConsoleHeight -= 2 - NumErrors;
            NumErrors++;
            var currentTop = Console.GetCursorPosition().Top;
            MaxCount--;
            if (CurrentAmount == MaxCount)
                Console.ForegroundColor = ConsoleColor.Yellow;
            AdjustPosition();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{CurrentAmount}/{MaxCount}");
            Console.SetCursorPosition(0, currentTop);
        }

        public void FixCursorPosition()
        {
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top + NumErrors);
        }

        public void Interrupt()
        {
            AdjustPosition();
            Console.ForegroundColor = ConsoleColor.Red;
            UM.PrintColoredText(ConsoleColor.Red, $"{CurrentAmount}/{MaxCount}", separateLines:true);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteFinished(bool newLine = true)
        {
            UM.PrintColoredText(ConsoleColor.Yellow, "Finished", separateLines:newLine);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteError()
        {
            UM.PrintColoredText(ConsoleColor.Red, "Error");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}