using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if HAS_ULTRAEDGE
using Teradyne.Igxl.Utilities;
#endif

namespace UltraEdgeTools
{

    /// <summary>
    /// Utilities to access the UltraEdge
    /// </summary>
    public class UETools
    {
        private UEProxy ue;

        /// <summary>
        /// Constructor. Initializes the handle to the UltraEdge object.
        /// </summary>
        public UETools()
        {
            ue = UEProxy.GetInstance();    
        }

        /// <summary>
        /// Communicates with the UltraEdge software to get any error from the last executed operation.
        /// If any, the error is printed to the console, otherwise "OK" is output.
        /// </summary>
        /// <param name="header">Header string to display before the status</param>
        private void printError(string header)
        {
#if HAS_ULTRAEDGE
            try
            {
                string status = ue.UltraEdge.CheckForErrors();
                if (String.IsNullOrEmpty(status)) status = "OK";
                Console.WriteLine($"{header}:{status}");
            }
            catch
            {
                // do nothing
            }
#else
            Console.WriteLine($"{header}:STUB - UltraEdge not available");
#endif
        }

        /// <summary>
        /// Send a docker file to the UltraEdge, load the image, and run the container.
        /// The docker file may be encrypted.
        /// </summary>
        /// <param name="dockerfilepath">Docker file to send</param>
        /// <param name="imagename">Docker image name</param>
        /// <param name="appname">UltraEdge application name</param>
        /// <param name="portMapping">Port mapping between the docker and the UltraEdge</param>
        /// <param name="dockerCommandLine">Application to run within the docker</param>
        /// <param name="workingDir">Parent folder containing the 'exec' subfolder used to share files between the docker and the UltraEdge</param>
        /// <param name="session_tag">Optional. Security tag for encrypted docker files.</param>
        public void LoadDockerFile(
            string dockerfilepath, string imagename, string appname,
            string portMapping, string dockerCommandLine, string workingDir, string session_tag = "")
        {
#if !HAS_ULTRAEDGE
            Console.WriteLine("ERROR: This is reference code that requires UltraEdge.dll to be installed.");
            Console.WriteLine("UltraEdge software is not available in this build environment.");
            return;
#else
            string dockerfilename = new FileInfo(dockerfilepath).Name;

            // Connect to the UltraEdge
            ue.UltraEdge.Connect();
            printError("Connect");

            // Start a session
            ue.UltraEdge.SecureSession.Start(session_tag);
            printError("Start session");

            if (String.IsNullOrEmpty(session_tag))
            {
                // Send Docker file
                ue.UltraEdge.WriteFile(dockerfilepath, dockerfilename);
                printError("Write docker file");
            }
            else
            {
                // Send Docker encrypted files
                ue.UltraEdge.WriteFile(dockerfilepath + ".enc", dockerfilename + ".enc");
                printError("Write docker encrypted file");
                ue.UltraEdge.WriteFile(dockerfilepath + ".key.enc", dockerfilename + ".key.enc");
                printError("Write encryption key file");

                // Decrypt the docker file
                ue.UltraEdge.DecryptFile(dockerfilename + ".enc");
                printError("Docker file decryption");
            }

            // Load docker image
            ue.UltraEdge.LoadDockerImage(imagename, dockerfilename);
            printError("Load docker image");

            // Run docker container
            ue.UltraEdge.Application(appname).RunDockerContainer(workingDir, dockerCommandLine, imagename, portMapping);
            printError("Run docker container");

            // End the session
            ue.UltraEdge.SecureSession.End();
            printError("End session");

            // Disconnect from the UltraEdge            
            try
            {
                ue.UltraEdge.Disconnect();
            }
            catch (PlatformNotSupportedException)
            {
                // Expected exception on .Net 8
                // Disconnect() uses Thread.Abort which is only available in .Net Framework
            }
            Console.WriteLine("Disconnected from the UltraEdge");
            // once disconnected, CheckErrors is not available anymore
#endif
        }
    }
}
