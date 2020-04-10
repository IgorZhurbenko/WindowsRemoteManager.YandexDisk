using System;
using System.Threading;
using System.Net.Mail;
using System.Net;
using WindowsRemoteManager;
using System.Collections.Generic;

namespace WindowsRemoteManager.YandexDisk
{
    
    class WindowsRemoteManagerMaster : WindowsRemoteManagerGeneral
    {
        
        public WindowsRemoteManagerMaster(string YaDiskToken, string BaseFolder, string YaDiskBaseFolder)
            : base(YaDiskToken, BaseFolder, YaDiskBaseFolder)
        {

        }

        private string GetReport(string CommandID)
        {
            try
            {
                return this.GetMessage("Report " + this.ID.ToString() + " " + CommandID);
            }
            catch
            {
                return "Error: message not acquired";
            }
        }

        private string RecordInstruction()
        {
            string EnteredLine;
            bool RecordingBat = false;
            string instruction = "";

            do
            {
                EnteredLine = Console.ReadLine();
                RecordingBat = (RecordingBat || EnteredLine.StartsWith("{")) && !EnteredLine.EndsWith("}");
                instruction = instruction + EnteredLine + '\n';
            }
            while (RecordingBat);

            return instruction;
        }

        private bool LoopAction(string Instruction)
        {

            try
            {
                string HashCode = Instruction.GetHashCode().ToString();

                this.SendMessage("Command " + this.ID.ToString() + " " + HashCode, Instruction);

                Thread.Sleep(RequestsInterval + 200);
                string report = "";
                do
                { report = GetReport(HashCode); Thread.Sleep(RequestsInterval + 100); }
                while (report.StartsWith("Error"));

                Console.WriteLine(report);

                return true;
            }
            catch { return LoopAction(Instruction); }

        }

        private void ClearAllReports()
        {
            
        }

        protected List<string> GetAllExecutives()
        {
            Dictionary<string, string>[] str = this.yandexDiskManager.GetFileStructure(this.YaDiskBaseFolder);

            List<string> ExecutivesList = new List<string>();

            foreach (Dictionary<string, string> Executive in str)
            {
                if ((Executive["name"].Length == 12) && (Executive["type"].ToLower() != "file"))
                {
                    string message = this.GetMessage(Executive["name"] + "/" + "Info", false);
                    ExecutivesList.Add(message);
                }
            }
            return ExecutivesList;
        }


        public void Launch()
        {
            List<string> RegisteredExecutives = this.GetAllExecutives();
                       

            if (!(RegisteredExecutives.Count < 1))
            {
                Console.WriteLine("Number|ID|NickName|DateOfLastAction|Status");   
                for (int a = 1; a<= RegisteredExecutives.Count; a++)
                { Console.WriteLine(a.ToString() + '|' + RegisteredExecutives[a-1]); }
                Console.WriteLine("Connection set. Here is the table of registered executives. Choose ID of the one you want to manage.");
            }

            else
            {
                Console.WriteLine("There are no registered executives. It is unlikely that any executive is operational now\n " +
                    "You may still enter an ID");
            }

            bool error = true;
            while (error)
            { 
                try 
                { 
                    this.ID = RegisteredExecutives[Convert.ToInt32(Console.ReadLine())-1].Split('|')[0];
                    this.YaDiskBaseFolder = this.YaDiskBaseFolder + "/" + this.ID;
                    error = false; 
                }
                catch { Console.WriteLine("Invalid value entered. Try only numbers this time."); }
            }
            ClearAllReports();

            int i = 1;

            while (true)
            {
                string instruction = RecordInstruction();
                LoopAction(instruction);
                i++;
            }
        }

    }

}

