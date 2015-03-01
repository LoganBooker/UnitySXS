using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace UnitySXS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Grab the version dictionary from user settings and make sure it's initialised.
            var versionDict = Properties.Settings.Default.UnityVersions;

            // We use a string dictionary, as .NET's user settings doesn't support Generics.
            // It's good enough for what we're doing anyway.
            if (versionDict == null)
                versionDict = new System.Collections.Specialized.StringDictionary();

            var args = Environment.GetCommandLineArgs();

            // The first element in the command args is the path to this executable.
            if (args.Length < 2)
            {
                MessageBox.Show("You must supply the path to a Unity editor executable (and optionally arguments for the editor).", "UnitySXS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Grab the Unity path from the arguments.
            var unityPath = args[1].Replace("\"", "");

            // Each subsequent argument is for Unity itself, so parse those for start info.
            var unityArgs = new StringBuilder(64);

            for (var i = 2; i < args.Length; i++)
            {
                unityArgs.Append(args[i]);

                if (i < args.Length - 1)
                    unityArgs.Append(" ");
            }
            
            // Check the editor executable exists.
            if (!File.Exists(unityPath))
            {
                MessageBox.Show("Supplied executable not found. Please make sure the file exists.", "UnitySXS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Grab version info.
            var unityVersion = FileVersionInfo.GetVersionInfo(unityPath);
            
            // Double-check we've grabbed the Unity Editor executable.
            if (string.IsNullOrEmpty(unityVersion.FileVersion) || unityVersion.FileDescription != "Unity Editor")
            {
                MessageBox.Show("Supplied executable is not the Unity Editor. Please double-check the file path.", "UnitySXS", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Create the version dictionary key
            var versionDictKey = string.Format("{0}.{1}.{2}", unityVersion.FileMajorPart, unityVersion.FileMinorPart, unityVersion.FileBuildPart);

            // Unity major versions use different registry keys, so we need to make sure
            // we're accessing the right one.
            var unitySubKeyName = string.Format("Software\\Unity Technologies\\Unity Editor {0}.x", unityVersion.FileMajorPart);

            RegistryKey unityRegistrySettings = null;

            try
            {
                // This can throw an exception, so catch it if needed.
                unityRegistrySettings = Registry.CurrentUser.OpenSubKey(unitySubKeyName, true);
            }
            catch (Exception ex) 
            {
                var error = string.Format("The following error occurred while trying to access Unity's registry settings:\n\n{0}", ex.Message);
                
                MessageBox.Show(error, "UnitySXS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (unityRegistrySettings == null)
            {
                MessageBox.Show("Unity's registry settings could not be accessed. You may not have sufficient system permissions.", "UnitySXS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var unityRegistryValueNames = unityRegistrySettings.GetValueNames();
            var unityProjectBasePathKeyName = "";

            foreach (var name in unityRegistryValueNames)
            {
                if (name.StartsWith("kProjectBasePath_"))
                {
                    unityProjectBasePathKeyName = name;
                    break;
                }
            }

            // We haven't stored any values yet, so clear the registry setting and run Unity.
            if (!Properties.Settings.Default.DoneFirstCheck || !versionDict.ContainsKey(versionDictKey))
            {
                // If the key/value pair was found, delete it.
                if (!string.IsNullOrEmpty(unityProjectBasePathKeyName))
                {
                    unityRegistrySettings.DeleteValue(unityProjectBasePathKeyName, false);
                    unityRegistrySettings.Flush();
                }

                Properties.Settings.Default.DoneFirstCheck = true;
                Properties.Settings.Default.Save();
            }
            else if (versionDict.ContainsKey(versionDictKey))
            {
                // Replace the registry value with the one we stored during a previous run.
                var storedProjectBasePath = versionDict[versionDictKey];
                unityRegistrySettings.SetValue(unityProjectBasePathKeyName, storedProjectBasePath);
            }

            // Start the Unity process so we can monitor termination.
            var unityProcess = new Process();
            var unityProcessStartInfo = new ProcessStartInfo();

            unityProcessStartInfo.Arguments = unityArgs.ToString();
            unityProcessStartInfo.FileName = unityPath;

            unityProcess.StartInfo = unityProcessStartInfo;
            unityProcess.EnableRaisingEvents = true;
            unityProcess.Start();

            // Wait while Unity is running.
            while (!unityProcess.HasExited)
                System.Threading.Thread.Sleep(1);

            unityProcess.Dispose();
            
            // Registry key name can change, so locate it again.
            unityRegistryValueNames = unityRegistrySettings.GetValueNames();
            
            foreach (var name in unityRegistryValueNames)
            {
                if (name.StartsWith("kProjectBasePath_"))
                {
                    unityProjectBasePathKeyName = name;
                    break;
                }
            }

            // Once Unity exits, save the project path.
            var currentBasePath = (string)unityRegistrySettings.GetValue(unityProjectBasePathKeyName, "");
            unityRegistrySettings.Close();
            
            // If the value is worth saving, then do it.
            if (!string.IsNullOrEmpty(currentBasePath))
            {
                if (!versionDict.ContainsKey(versionDictKey))
                    versionDict.Add(versionDictKey, currentBasePath);
                else
                    versionDict[versionDictKey] = currentBasePath;
            }

            // Store the settings for access later.
            Properties.Settings.Default.UnityVersions = versionDict;
            Properties.Settings.Default.Save();
        }
    }
}
