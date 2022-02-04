using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using Common.SMTP;
using System.Collections;
using System.Collections.Generic;

namespace SMTPServer {
    public class Listener {
        System.IO.StreamReader reader;
        System.IO.StreamWriter writer;
        TcpClient client;

        #region Main and Constructor 
        public Listener(TcpClient client) {
            this.client = client;
            NetworkStream stream = client.GetStream();
            reader = new System.IO.StreamReader(stream);
            writer = new System.IO.StreamWriter(stream);
            writer.NewLine = "\r\n";
            writer.AutoFlush = true;
        }
        public static void Start() {
            //TcpListener listener = new TcpListener(IPAddress.Parse(SMTPServer.ListenerIP), SMTPServer.ListenerPort);
            TcpListener listener = new TcpListener(IPAddress.Any, SMTPServer.ListenerPort);

            listener.Start();
            while (true) {
                Listener handler = new Listener(listener.AcceptTcpClient());
                Thread thread = new System.Threading.Thread(new ThreadStart(handler.Run));
                thread.Start();
            }
        }
        #endregion
        public void Run() {
            try {
                wr(220, "cristianavaleca.com --  SMTP Server");
                SMTPMessage message = new SMTPMessage();
                bool isUserAuthenticated = false;
                for (; ; ) {
                    string line = rd();
                    if (line == null)
                        break;
                    //Console.WriteLine("RawLine:      " + line);
                    string[] tokens = line.Split(' ');
                    bool requiresAuthorization = false;
                    switch (tokens[0].ToUpper()) {
                       
                        case "EHLO":
                            wr(250, "AUTH LOGIN PLAIN");
                            break;
                        case "HELP":
                            wr(250, "OK Success - please contact *insert site* ");
                            break;
                        case "HELO":
                            wr(250, "OK Success");
                            break;
                        case "MAIL":
                            if (SMTP.checkRCPTIsInternal(line))
                                requiresAuthorization = true;
                            if (requiresAuthorization && !isUserAuthenticated) {
                                wr(530, "Authorization requiredw");
                                break;
                            }
                            message.From = line;
                            wr(250, "OK Success");
                            break;
                        case "RCPT":
                            if (!SMTP.checkRCPTIsInternal(line)) {
                                requiresAuthorization = true;
                                //break;
                            }
                            if (requiresAuthorization && !isUserAuthenticated) {
                                wr(530, "Authorization required");
                                break;
                            }
                            message.To.Add(line);
                            wr(250, "OK ");
                            break;
                        case "AUTH":
                            //wr(334, "VXNlcm5hbWU6");
                            //string user = rd64();
                            //if (user == null)
                            //    return;
                            //if (user.Length == 0) {
                            //    wr(535, "invalid user");
                            //    break;
                            //}
                            //wr(334, "UGFzc3dvcmQ6");
                            //string pass = rd64();
                            //if (pass == null)
                            //    return;
                            //if (pass.Length == 0) {
                            //    wr(535, "invalid password");
                            //}
                            //if (SMTPServer.UserPass.Equals(user + "-" + pass)) {
                            //    wr(535, "Authentication failed");
                            //    break;
                            //}
                            isUserAuthenticated = true;
                            wr(235, "Authentication succesful");
                            break;
                        case "DATA":
                            if (requiresAuthorization && !isUserAuthenticated) {
                                wr(530, "Authorization Required");
                                break;
                            }
                            wr(354, "End data with <CR><LF>.<CR><LF>");
                            message.Data.Add(line);
                            for (; ; ) {
                                line = rd();
                                message.Data.Add(line);
                                if ((line == null) || (line == "."))
                                    break;
                            }
                            wr(250, "Ok: queued");
                            message.SaveAndSend();
                            message = new SMTPMessage();
                            //Console.WriteLine("sd");
                            break;
                        case "RSET":
                            wr(250, "Ok: Reset");
                            message = new SMTPMessage();
                            break;
                        case "QUIT":
                            wr(221, "BYE");
                            message.SaveAndSend();
                            writer.Close();
                            client.Close();
                            return;
                        default:
                            wr(550, "Command not understood");
                            break;
                    }
                }
            }
            catch (Exception) { }
        }
        private void wr(int code, string c) {
            writer.WriteLine(code + " " + c);
            writer.Flush();
            Console.WriteLine(">> " + code + " " + c);
        }
        private string rd() {
            string result = null;
            try {
                //Console.WriteLine(reader.EndOfStream);
                result = reader.ReadLine();
            }
            catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
            }
            if (result == null)
                Console.WriteLine("<< NULL");
            else {

                Console.WriteLine("<< " + result);
                File.AppendAllText("mailHeader.txt", result);
                File.AppendAllText("mailHeader.txt", "\n");
            }
            return result;
        }
        private string rd64() {
            try {
                string record = rd();
                if (record == null)
                    return null;
                return System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(record));
            }
            catch (Exception) {
                return "";
            }
        }
    }
}