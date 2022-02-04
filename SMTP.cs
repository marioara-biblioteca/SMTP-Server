using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Common.SMTP {
    class SMTP {

        //gaseste toate emailurile dintr un text si retunreaza un array cu ele
        public static string[] extractMails(string text) {
            Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*",
            RegexOptions.IgnoreCase);
            MatchCollection emailMatches = emailRegex.Matches(text);
            List<string> result = new List<string>();
            foreach (Match emailMatch in emailMatches)
                result.Add(emailMatch.Value);
            return result.ToArray();
        }
        public static bool checkRCPTIsInternal(string text) {
            string[] mails = extractMails(text);
            foreach (string mail in mails)
                if (!isInternal(mail))
                    return false;
            return true;
        }
        public static bool isInternal(string email) {
            if (email.EndsWith("@cristianavaleca.com"))
                return true;
            if (email.EndsWith("@localhost"))
                return true;
            if (email.EndsWith("@localhost.com"))
                return true;
            return false;
        }
    }
    class SMTPMessage {
        public string From;
        public List<string> To = new List<string>();
        public List<string> Data = new List<string>();
        internal void SaveAndSend() {
            Save();
            string username;
            int pos;
            foreach (var to in this.To) 
            {
                var mail = SMTP.extractMails(to);
                if (!SMTP.isInternal(mail[0]))
                    SMTPServer.Sender.Send(mail[0], Data, From);
                else
                {
                    username = SMTP.extractMails(To[0])[0];
                    pos = username.IndexOf('@');
                    username = username.Substring(0, pos);
                    SaveInternal(username, true);
                }
            }
            if(SMTP.isInternal(From))
            {
                username = SMTP.extractMails(From)[0];
                pos = username.IndexOf('@');
                username = username.Substring(0, pos);
                SaveInternal(username, false);
            }

        }
        
        internal void Save() {
            DirectoryInfo directory = SMTPServer.SMTPServer.ReceivedPath;
            directory.Create();
            string uniqueFileName = Path.Combine(directory.FullName, System.Guid.NewGuid().ToString() + ".smtp");
            File.WriteAllLines(uniqueFileName, To);
            File.AppendAllLines(uniqueFileName, Data);
        }
        
        void SaveInternal(string username,bool isInbox)
        {
            
            
            string mailboxPath = Path.Combine(SMTPServer.SMTPServer.InboxPath, username);
            if(!isInbox)
            {
                mailboxPath = Path.Combine(mailboxPath, "sent");
            }

            bool existaUser = false;
            foreach(var user in File.ReadAllLines(SMTPServer.SMTPServer.Credentials))
            {
                string currentUser = user.Split(' ')[0];
                if(username == currentUser)
                {
                    existaUser = true;
                    break;
                }
            }
            if (existaUser == false)
                return;

            DirectoryInfo directory = new DirectoryInfo(mailboxPath);
            directory.Create();

            FileInfo[] files = directory.GetFiles();
            int lastUID = 0;
            foreach (var currentFile in files)
            {
                string localUID = currentFile.Name.Split('.')[0];
                if (Int32.Parse(localUID) > SMTPServer.SMTPServer.UID)
                    SMTPServer.SMTPServer.UID = Int32.Parse(localUID);
            }
            SMTPServer.SMTPServer.UID++;

            string filename = Path.Combine(mailboxPath, SMTPServer.SMTPServer.UID.ToString() + ".txt");
            var file = File.Create(filename);
            file.Close();
            File.AppendAllLines(filename, Data.GetRange(1, Data.Count - 1));
            SMTPServer.SMTPServer.UID++;
        }
    }
}