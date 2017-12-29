using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace PHP_Server
{
    class Program
    {
        static int port = 1500;
        static string host = "127.0.0.1"; //home sweet home 
        static void Main(string[] args)
        {
            HandleConnection();
        }
        static void HandleConnection()
        {

            while (true)
            {
                
                TcpListener listener = new TcpListener(IPAddress.Parse(host), port);
                listener.Start();
                TcpClient client = listener.AcceptTcpClient(); 
                NetworkStream networkStream1 = client.GetStream();
                StreamReader reader = new StreamReader(networkStream1);
                string requestHeader = reader.ReadLine();
                if (string.IsNullOrEmpty(requestHeader))
                {
                    Console.WriteLine("INVALID REQUEST");
                    return;
                }
                string[] requests = requestHeader.Split(' ');
                string page = requests[1].Substring(1);
                if (string.IsNullOrEmpty(page))
                {
                    Console.WriteLine("NO PAGE REQUESTS");
                    page = "index.abc";//default page
                }
                StreamWriter networkStream = new StreamWriter(networkStream1);
                if (!File.Exists(page))
                {
                    Console.WriteLine("PAGE NOT FOUND");
                    networkStream.Write("HTTP/1.0 404 OK");
                    networkStream.Write(Environment.NewLine);
                }
                else
                {
                    StreamReader streamReader = new StreamReader(page);
                    string cevap = Interpreter(streamReader.ReadToEnd());
                    Console.WriteLine("Connected,sending data...");
                    networkStream.Write("HTTP/1.0 200 OK");
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write("Content-Type: text/html; charset=UTF-8");
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write("Content-Length: " + cevap.Length);
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write("<!DOCTYPE html>");
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write("<html><body>");
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write(cevap);
                    networkStream.Write(Environment.NewLine);
                    networkStream.Write("</body></html>");
                }
                networkStream.Flush();
                reader.Close();
                client.Close();
                listener.Stop();

            }
        }
        private static string Interpreter(string readedString)
        {
            readedString = readedString.Replace("\n", String.Empty);
            string[] interpretation = readedString.Split(';');
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var line in interpretation)
            {
                if (string.IsNullOrEmpty(line)) //son ; tan sonraki satırda
                    continue;
                if (line.Contains("WriteBold"))
                {
                    stringBuilder.Append("<b>" + line.Replace("WriteBold(\"", "").Replace("\")","") + "</b><br>");
                }
                else if (line.Contains("WriteHeadLine"))
                {
                    stringBuilder.Append("<h1>" + line.Replace("WriteHeadLine(\"", "").Replace("\")", "") + "</h1><br>");
                }
                else
                {
                    if (!line.Contains("Write"))
                    {
                        return "SYNTAX ERROR";
                    }
                    else
                    {
                        stringBuilder.Append(line.Replace("Write(\"", "").Replace("\")", "") + "<br>");
                    }
                }
            }
            return stringBuilder.ToString();
        }

    }
}
