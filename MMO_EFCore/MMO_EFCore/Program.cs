using System;


namespace MMO_EFCore
{
    class Program
    {
        
        static void Main(string[] args)
        {
            DbCommands.InitializeDB(forceReset: true);

            // CRUD (Create-Read-Update-Delete)
        }
    }
}
