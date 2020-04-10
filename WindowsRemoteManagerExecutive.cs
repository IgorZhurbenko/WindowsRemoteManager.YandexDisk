using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.Mail;
using System.Net;
using System.Text.Json;

namespace WindowsRemoteManager.YandexDisk
{
    class WindowsRemoteManagerExecutive : WindowsRemoteManagerGeneral
    {        
        public string NickName = "NickName";
        
        
        public WindowsRemoteManagerExecutive(string YaDiskToken, string BaseFolder, string YaDiskBaseFolder)
            : base(YaDiskToken, BaseFolder, YaDiskBaseFolder)
        {
            this.ID = this.GetMac().Replace("-", "");
        }

        
        private List<string> ExecuteBat(Command command)
        {

            List<string> result = new List<string>();
            string BatContent = "";

            foreach (string Instruction in command.Instructions)
            {
                if (Instruction.StartsWith("{"))
                {
                    BatContent = BatContent + '\n' + Instruction.Replace("{", "").Replace("}","");
                }
                else if (Instruction.EndsWith("}") || Instruction.EndsWith("}\r"))
                {
                    BatContent = BatContent + '\n' + Instruction.Replace("}", "").Replace("{","");
                    break;
                }
                else 
                {
                    BatContent = BatContent + '\n' + Instruction;
                }
            }

            string FileName = this.BaseFolder + @"\" + "Command " + this.ID.ToString() + " " + command.ID.ToString() + ".bat";

            if (File.Exists(FileName)) { File.Delete(FileName); }

            File.AppendAllText(FileName, BatContent);

            ProcessStartInfo psiOpt = new ProcessStartInfo(@FileName);
            psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
            psiOpt.RedirectStandardOutput = true;
            psiOpt.UseShellExecute = false;
            psiOpt.CreateNoWindow = true;
            Process procCommand = Process.Start(psiOpt);
            StreamReader sr = procCommand.StandardOutput;
            result.Add(sr.ReadToEnd());
            File.Delete(FileName);
            return result;
        }
        
        private List<string> Execute(Command command)
        {
            List<string> result = new List<string>();

            if (command.Instructions[0].StartsWith("{"))
            {
                return this.ExecuteBat(command);
            }

            foreach (string Instruction in command.Instructions)
            {
                
                if (!Instruction.StartsWith(@"'") && !Instruction.StartsWith(@"{"))
                {
                    ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C " + @Instruction);
                    psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                    psiOpt.RedirectStandardOutput = true;
                    psiOpt.UseShellExecute = false;
                    psiOpt.CreateNoWindow = true;
                    Process procCommand = Process.Start(psiOpt);
                    StreamReader sr = procCommand.StandardOutput;
                    result.Add(sr.ReadToEnd());
                }
                else if (Instruction.StartsWith(@"'curl"))
                {
                    try
                    {
                        ProcessStartInfo psiOpt = new ProcessStartInfo(@"curl.exe", /*@"/C " +*/ @Instruction.Replace(@"'curl", ""));
                        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                        psiOpt.RedirectStandardOutput = true;
                        psiOpt.UseShellExecute = false;
                        psiOpt.CreateNoWindow = true;
                        Process procCommand = Process.Start(psiOpt);
                        StreamReader sr = procCommand.StandardOutput;
                        result.Add(sr.ReadToEnd());
                    }
                    catch (Exception error) { result.Add(error.Message); }
                }
                else if (Instruction.ToLower().StartsWith(@"'setrequestsinterval"))
                {
                    string IntervalString = Instruction.Split(" ")[1].Trim();
                    try
                    {
                        int Interval = Convert.ToInt32(IntervalString);
                        if (Interval < 100) { Interval = 100; }
                        if (Interval > 3600000) { Interval = 3600000; }
                        this.RequestsInterval = Interval;
                        result.Add("Requests interval set to " + Interval.ToString());
                    }
                    catch { result.Add("Wrong input for requests interval"); }

                }
                else if (Instruction.ToLower().StartsWith(@"'setnickname"))
                {
                    if (Instruction.Split(" ").Length > 1)
                    {
                        this.NickName = Instruction.Split(" ")[1].Trim();
                        result.Add("Nickname of the executive changed to " + this.NickName);
                        this.RenewExecutiveInfo();
                    }
                }
            }
            return result;
        }
      
        private List<Command> GetCommands()
        {
            
            List<Command> Commands = new List<Command>();
            Dictionary<string, string>[] Structure = this.yandexDiskManager.GetFileStructure(this.YaDiskBaseFolder);
            
            foreach (Dictionary<string,string> file in Structure) 
            {
                if (file["name"].StartsWith("Command " + this.ID.ToString()))
                {
                    Commands.Add(new Command(file["name"].Split(" ")[2], 
                        this.GetMessage(file["name"]).Split('\n') ));
                }
            }

            return Commands;

        }

        public string GetLogFile()
        {
            return this.BaseFolder + @"/log.txt";
        }
               
        private void LoopAction()
        {
            foreach (Command command in this.GetCommands())
            {
                File.AppendAllText(this.GetLogFile(), @"***" + command.ID.ToString() + "***" + '\n');
                List<string> results = this.Execute(command);
                string message = String.Join('\n', results);
                File.AppendAllText(this.GetLogFile(), message);
                                
                this.SendMessage("Report " + this.ID + " " + command.ID, message);
            }
        }

        public void Launch()
        {
            
            this.RegisterExecutive();

            Console.WriteLine("Connection set. Executing commands...");
            while (true)
            {
                this.LoopAction();
                Thread.Sleep(RequestsInterval);
            }
        }

        protected enum RegistrationOption
        {
            Upload,
            Accept
        }

        private void RegisterExecutive(RegistrationOption RO = RegistrationOption.Accept)
        {
            string FolderCreatedMessage = this.yandexDiskManager.CreateFolder(this.YaDiskBaseFolder + "/" + this.ID);
            this.YaDiskBaseFolder = this.YaDiskBaseFolder + "/" + this.ID;
            string Info = this.GetMessage("Info");
            if (!Info.StartsWith("Error"))
            {
                try { this.NickName = Info.Split('|')[1];} catch { this.NickName = "NickName"; }
            }
            else { this.NickName = "NickName"; }
            RenewExecutiveInfo();
        }

        private void RenewExecutiveInfo()
        {
            string message = this.ID + '|' + this.NickName + '|' + DateTime.Now.ToString() + '|' + "Active";
            this.yandexDiskManager.DeleteFile(this.YaDiskBaseFolder + "/" + "Info");
            this.SendMessage("Info", message);            
        }

        private void UploadInfo()
        {
            
        }
        protected string GetMac()
        {
            ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C " + "getmac");
            psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
            psiOpt.RedirectStandardOutput = true;
            psiOpt.UseShellExecute = false;
            psiOpt.CreateNoWindow = true;
            Process procCommand = Process.Start(psiOpt);
            StreamReader sr = procCommand.StandardOutput;
            return sr.ReadToEnd().Split('\n')[3].Split(" ")[0];
        }
    }
}
