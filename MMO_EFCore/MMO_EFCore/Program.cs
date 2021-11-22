using System;


namespace MMO_EFCore
{
    class Program
    {
        
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
                Console.WriteLine("[1] Update (Reload)");
                Console.Write(" > ");
                string command = Console.ReadLine();
                switch (command)
                {
                    case "0":
                        DbCommands.InitializeDB(forceReset: true);
                        break;
                    case "1":
                        DbCommands.Test();
                        break;
                    case "2":
                        break;
                    case "3":
                        break;
                }
                Console.WriteLine();
            }
        }
    }
}
