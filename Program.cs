using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using Common.DNS;
using System.Net.Sockets;

namespace SMTPServer {
    public class SMTPServer {
        static Dictionary<string, string> configuration = new Dictionary<string, string>();
        public static int UID;
        static void Fatal(string message) {
            Console.WriteLine("FATAL: " + message);
            Console.ReadKey();
            Environment.Exit(1);
        }
        public static void Main(string[] args) {


            // TcpClient client = new TcpClient("smtp.google.com", 25);
            UID = 1;

            foreach (var arg in args) {
                var kv = arg.Split('=');
                configuration[kv[0]] = kv[1];
            }
            Console.WriteLine("IP: " + ListenerIP);
            Console.WriteLine("Port: " + ListenerPort);
            new System.Threading.Thread(new ThreadStart(Listener.Start)).Start();
        }
        public static string ListenerIP {
            get {
                return config("listener-ip");
            }
        }
        public static int ListenerPort {
            get {
                int val;
                var strVal = config("listener-port");
                if (!int.TryParse(strVal, out val))
                    Fatal("e1204142034 - Config listener-port defined as 'listener-port=" + strVal + "' in command line must be numeric");
                return val;
            }
        }
        public static string UserPass {
            get {
                return config("user-pass");
            }
        }
        public static DirectoryInfo ReceivedPath {
            get {
                return new DirectoryInfo(config("received-path"));
            }
        }

        public static string InboxPath
        {

            get
            {
                return config("inbox-path");
            }
        }

        public static string Credentials
        {
            get
            {
                return config("credentials");
            }
        }

        private static string config(string name) {
            if (!configuration.ContainsKey(name))
                Fatal("e1204142032 - Config " + name + " not defined as '" + name + "=<value>' in command line");
            return configuration[name];
        }
    }
}