using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace SAP_WoNo_Infomation_MES
{
    class Program
    {
        static List<string> strList = new List<string>();
        static void Main(string[] args)
        {
            Console.WriteLine("資料寫入中.....");
            //取得FTP文件列表
            Dir_File();
            //下載FTP文件
            Download_TXT();
            //讀取並寫入資料庫
            TxT_Read_Write();
            //刪除FTP檔案
            //Delete_File();
            Console.WriteLine("\n\n\n\n" + "寫入完畢,按任意建關閉!!");
            //Console.ReadKey();
        }
        public class ImpersonateUser : IDisposable
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

            [DllImport("kernel32", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr hObject);

            private IntPtr userHandle = IntPtr.Zero;
            private WindowsImpersonationContext impersonationContext;

            public ImpersonateUser(string user, string domain, string password)
            {
                if (!string.IsNullOrEmpty(user))
                {
                    // Call LogonUser to get a token for the user
                    bool loggedOn = LogonUser(user, domain, password,
                        9 /*(int)LogonType.LOGON32_LOGON_NEW_CREDENTIALS*/,
                        3 /*(int)LogonProvider.LOGON32_PROVIDER_WINNT50*/,
                        out userHandle);
                    if (!loggedOn)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Begin impersonating the user
                    impersonationContext = WindowsIdentity.Impersonate(userHandle);
                }
            }

            public void Dispose()
            {
                if (userHandle != IntPtr.Zero)
                    CloseHandle(userHandle);
                if (impersonationContext != null)
                    impersonationContext.Undo();
            }
        }
        //登入→取得清單
        static void Dir_File()
        {
            //工單資訊-20220921020032
            strList.Clear();
            try
            {
                //using (new ImpersonateUser("sap-mes", @"\\192.168.4.85\", "Eversun@888"))
                //{                    
                //}
                DirectoryInfo info = new DirectoryInfo(@"\\192.168.4.85\saploc\SAP_Export\PP\WO_info\");
                foreach (var item in info.GetFiles())
                {
                    string sad = item.Name.Substring(5, 8);
                    if (item.Name.Substring(5, 8) == DateTime.Now.ToString("yyyyMMdd"))
                    {
                        strList.Add(item.Name);
                    }
                }
            }
            catch (Exception i)
            {
                Console.WriteLine(i.Message);
            }
        }
        //下載至本機程式路徑
        static void Download_TXT()
        {
            string sourcePath = @"\\192.168.4.85\saploc\SAP_Export\PP\WO_info\";
            string download_Path = Environment.CurrentDirectory + "\\wo_info\\";
            for (int i = 0; i < strList.Count; i++)
            {
                try
                {
                    using (new ImpersonateUser("sap-mes", @"\\192.168.4.85\", "Eversun@888"))
                    {
                        File.Copy(sourcePath + strList[0], download_Path + strList[0], true);
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        //讀取本機程式路徑TXT檔
        static void TxT_Read_Write()
        {
            String line;
            string wo_info = string.Empty;
            string eng_info = string.Empty;
            string localPath = Environment.CurrentDirectory + "\\wo_info\\";
            string rctime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                DirectoryInfo di = new DirectoryInfo(localPath);
                //textBox1.Clear();
                //textBox1.Text += "執行時間 : " + specDate;
                foreach (var fi in di.GetFiles())
                {
                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader(localPath + fi.Name);
                    //Read the first line of text
                    line = sr.ReadLine();
                    //Continue to read until you reach end of file
                    while (line != null)
                    {
                        string[] line_Item = line.Split('\t');
                        //資料分析
                        if (line_Item[0] == "INFO")
                        {
                            wo_info = line_Item[1];
                            eng_info = line_Item[2];
                            string sql = "select * from JH_SAP_TO_MES where WO_NO=@WO_NO  and ENG_SR=@ENG_SR ";
                            SqlParameter[] pms = new SqlParameter[]
                            {
                                new SqlParameter("@WO_NO", wo_info),
                                new SqlParameter("@ENG_SR", eng_info)
                            };
                            SqlDataReader dr = db.ExecuteReader(sql, CommandType.Text, pms);
                            if (!dr.Read())
                            {
                                string insSql = "insert into JH_SAP_TO_MES values( @WO_NO ,@ENG_SR ,@rcTime) ";
                                SqlParameter[] parm2 = new SqlParameter[]
                                {
                                    new SqlParameter("@WO_NO", wo_info),
                                    new SqlParameter("@ENG_SR", eng_info),
                                    new SqlParameter("@reTime",rctime)
                                };
                                db.ExecueNonQuery(insSql, CommandType.Text, parm2);
                                Console.WriteLine(wo_info + "\n" + eng_info);
                            }
                        }
                        //Read the next line
                        line = sr.ReadLine();
                    }
                    //close the file
                    sr.Close();
                    //Console.ReadLine();
                    //textBox1.Text += Environment.NewLine;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
        //刪除本機程式路徑TXT檔
        static void Delete_File()
        {
            string localPath = Environment.CurrentDirectory + "\\wo_info\\";
            string[] txtList = Directory.GetFiles(localPath, "*.txt");

            try
            {
                foreach (var fi in txtList)
                {
                    System.IO.File.Delete(fi);
                    //Console.WriteLine(fi + "\n");
                }
            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
        }
    }
}
