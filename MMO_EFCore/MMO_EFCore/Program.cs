using Microsoft.EntityFrameworkCore;
using System;


namespace MMO_EFCore
{
    class Program
    {
        // Annotion (Attribute)
        [DbFunction()]
        public static double? GetAverageReviewScore(int itemId)
        {
            throw new NotImplementedException("사용 금지!");
        }
        static void Main(string[] args)
        {
            DbCommands.InitializeDB(forceReset: true);

            // CRUD (Create-Read-Update-Delete)
            //Console.WriteLine("명령어를 입력하세요");
            //Console.WriteLine("[0] Force Reset");
            //Console.WriteLine("[1] Eager Loading"); // 즉시
            //Console.WriteLine("[2] Explicit Loading"); // 명시적
            //Console.WriteLine("[3] Select Loading"); // 선택적

            while(true)
            {
                Console.WriteLine("명령어를 입력하세요");
                Console.WriteLine("[0] Force Reset");
                Console.WriteLine("[1] ShowItems");
                Console.WriteLine("[2] CalcAverage");
                Console.Write(" > ");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "0":
                        DbCommands.InitializeDB(forceReset: true);
                        break;
                    case "1":
                        DbCommands.ShowItems();
                        break;
                    case "2":
                        DbCommands.CalcAverage();
                        break;
                    case "3":
                        break;
                }
                Console.WriteLine();
            }
        }
    }
}
