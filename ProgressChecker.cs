
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
            Console.Write($"{message}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{CurrentAmount}/{MaxCount}");
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
            Console.WriteLine($"{CurrentAmount}/{MaxCount}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteFinished(bool newLine = true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (newLine)
                Console.WriteLine("Finished");
            else
                Console.Write("Finished");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteError()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}