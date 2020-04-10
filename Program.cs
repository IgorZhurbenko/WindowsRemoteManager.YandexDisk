using System;
using System.Threading.Tasks;

namespace WindowsRemoteManager.YandexDisk
{
    class Program
    {

        static void Main(string[] args)
        {
            string YaDiskToken = "xxx";
            Console.WriteLine("Type 1 to launch Windows remote executive and 2 to launch Windows remote master.");
            Char key = Console.ReadKey().KeyChar;
            Console.WriteLine();
            
            if (key == '1')
            {
                WindowsRemoteManagerExecutive WRME = new WindowsRemoteManagerExecutive(YaDiskToken,@"C:\Users\Public\Pictures", "WindowsRemoteManager");

                WRME.Launch();
            }
            else if (key == '2')
            {
                WindowsRemoteManagerMaster WRMM = new WindowsRemoteManagerMaster(YaDiskToken, @"C:\Users\Public\Pictures", "WindowsRemoteManager");
                WRMM.Launch();
            }
            else
            {
                Console.WriteLine("Wrong symbol. Try again.");
                Main(args);
            }
        }
    }
}
