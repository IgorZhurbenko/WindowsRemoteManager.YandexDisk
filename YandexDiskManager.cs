using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;




namespace WindowsRemoteManager.YandexDisk
{
    class YandexDiskManager
    {
        public readonly string Token;
        private HttpClient httpClient;
        private WebClient webClient;
        private string ComputerBasePath;

        public YandexDiskManager(string Token, ref HttpClient httpClient, ref WebClient webClient, string ComputerBasePath = @"c:\users\public\pictures")
        {
            this.Token = Token;
            this.ComputerBasePath = ComputerBasePath;
            this.httpClient = httpClient;
            this.webClient = webClient;
        }   

        public YandexDiskManager(string Token, string ComputerBasePath = @"c:\users\public\pictures")
        {
            this.Token = Token;
            this.ComputerBasePath = ComputerBasePath;
            this.httpClient = new HttpClient();
            this.webClient = new WebClient();
        }
        public Dictionary<string, string>[] GetFileStructure(string YandexDiskDirectory)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("Get"), "https://cloud-api.yandex.net/v1/disk/resources?path=/" + YandexDiskDirectory + "&fields=" + "_embedded.items.name,_embedded.items.type" + "&limit=100"))
            {
                request.Headers.TryAddWithoutValidation("Authorization", "OAuth " + this.Token);
                HttpResponseMessage Response = httpClient.SendAsync(request).Result;
                string JSONResult = Response.Content.ReadAsStringAsync().Result;
                Dictionary<string, string>[] str =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>[]>>>(JSONResult)["_embedded"]["items"];
                return str;
            }
        }

        public string DownloadFile(string YandexDiskPath, string ComputerPath)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Get"),
                    "https://cloud-api.yandex.net/v1/disk/resources/download?path=/" + YandexDiskPath + "/"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "OAuth " + this.Token);

                    HttpResponseMessage Response = httpClient.SendAsync(request).Result;

                    string DownloadURL = JsonSerializer.Deserialize<Dictionary<string, object>>(Response.Content.ReadAsStringAsync().Result)["href"].ToString();

                    if (File.Exists(ComputerPath)) { File.Delete(ComputerPath); }

                    this.webClient.DownloadFile(new Uri(DownloadURL), ComputerPath);

                    if (File.Exists(ComputerPath))
                    { return "Success: File downloaded"; }
                    else 
                    { return "Error: File not downloaded"; }
                }
                
               
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public string UploadFile(string YandexDiskPath, string FilePath, string ResultantFileName)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Get"),
                    "https://cloud-api.yandex.net/v1/disk/resources/upload?path=/" + YandexDiskPath + "/" + ResultantFileName + "/"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "OAuth " + this.Token);

                    HttpResponseMessage Response = httpClient.SendAsync(request).Result;

                    string UploadURL = JsonSerializer.Deserialize<Dictionary<string, object>>(Response.Content.ReadAsStringAsync().Result)["href"].ToString();

                    string result = String.Join(" ",this.webClient.UploadFile(UploadURL, FilePath));

                    return result;
                }
                
            }
            catch { return "Error: file not uploaded"; }
        }

        public string CreateFolder(string NewFolderPath)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Put"),
                    "https://cloud-api.yandex.net/v1/disk/resources/?path=/" + NewFolderPath + "/"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "OAuth " + this.Token);

                    return httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                    

                }
                
            }
            catch { return "Error: "; }
        }

        //Case of the File path matters
        public string DeleteFile(string FileToDeletePath)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://cloud-api.yandex.net/v1/disk/resources?path=" + "/" + FileToDeletePath))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "OAuth " + this.Token);
                    string Response = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                    return Response;
                }
                
            }
            catch { return "Error: file wasn't deleted"; }
        }
        public void DeleteFileAsync(string FileToDeletePath)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("DELETE"), "https://cloud-api.yandex.net/v1/disk/resources?path=" + "/" + FileToDeletePath))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "OAuth " + this.Token);
                    string Response = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                    
                }

            }
            catch { }
        }

        public string ReadFileFrom(string YandexDiskPath)
        {
            string FileName = YandexDiskPath.Split('/')[YandexDiskPath.Split('/').GetUpperBound(0)];
            DownloadFile(YandexDiskPath, ComputerBasePath + @"\" + FileName);
            try
            {
                string result = File.ReadAllText(ComputerBasePath + @"\" + FileName);
                File.Delete(ComputerBasePath + @"\" + FileName);
                return result;
            }
            catch { return "Error: file not found"; }
        }

    }
}
