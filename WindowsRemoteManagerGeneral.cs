using System;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;


namespace WindowsRemoteManager.YandexDisk
{
    abstract class WindowsRemoteManagerGeneral

    {
        protected string BaseFolder;
        public YandexDiskManager yandexDiskManager;
        protected int RequestsInterval = 100;
        protected string YaDiskBaseFolder;
        protected string ID;
        protected WebClient webClient;
        protected HttpClient httpClient;

        protected WindowsRemoteManagerGeneral(string YaDiskToken, string BaseFolder, string YaDiskBaseFolder)
        {
            this.httpClient = new HttpClient();
            this.webClient = new WebClient();
            this.yandexDiskManager = new YandexDiskManager(YaDiskToken, ref this.httpClient, ref this.webClient);
            this.BaseFolder = BaseFolder;
            this.YaDiskBaseFolder = YaDiskBaseFolder;
            
        }

        public bool CheckConnection()
        {
            return true;
        }
                
        protected void SendMessage(string FileName, string FileContent)
        {
            string FilePath = this.BaseFolder + @"\" + FileName;
            if (File.Exists(FilePath))
            {
                try { File.Delete(FilePath); } catch { }
            }
            File.AppendAllText(FilePath, FileContent);
            this.yandexDiskManager.UploadFile(this.YaDiskBaseFolder, FilePath, FileName);
            File.Delete(FilePath);
        }

        protected string GetMessage(string YandexDiskMessageName, bool DeleteAfterReading = true)
        {
            string[] SplittedPath = YandexDiskMessageName.Split('/');
            string FilePath = this.BaseFolder + @"\" +  SplittedPath[SplittedPath.Length - 1];
            string result = ""; 
            this.yandexDiskManager.DownloadFile(this.YaDiskBaseFolder + "/" + YandexDiskMessageName, FilePath);
            try
            {
                result = File.ReadAllText(FilePath); File.Delete(FilePath);
                if (DeleteAfterReading) { this.yandexDiskManager.DeleteFile(this.YaDiskBaseFolder + "/" + YandexDiskMessageName); }
                return result;
            }
            catch { return @"Error: message wasn't acquired"; }
           
        }

        

        

    }
}

