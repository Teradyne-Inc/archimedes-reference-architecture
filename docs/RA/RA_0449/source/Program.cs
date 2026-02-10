using System;
using UltraEdgeTools;

namespace DockerDeployer
{
    internal class Program
    {
        private const string DockerFilePath = @"C:\temp\pydas.tar.gz";
        private const string DockerImageName = "anyname"; // used for reference internally in the code, but the image name is the one chosen at Docker file creation
        private const string UEAppName = "alsoanyname"; // used for reference internally in the code
        private const string DockerPortMapping = "5001:5001,5672:5672"; // two port mappings defined: 5001 is the DAS port (the docker is a DAS server), 5672 to send adaptive commands (the docker is also a RabbitMQ client)
        private const string CommandToExecute = "/app/pydas.py"; // command to run inside the docker, can be empty if CMD is used in the Dockerfile
        private const string WorkingDir = ""; // folder used for file sharing between the docker and the UltraEdge host. A subfolder "exec" is automatically added.

        private const bool ENCRYPTION = false;
        private const string SESSION_TAG = "DECRYPT_KEY"; // tag previously created when installing a private key

        static void Main(string[] args)
        {
            // UETools ue = UETools.GetInstance();
            UETools ue = new UETools();

            string sessiontag = ENCRYPTION ? SESSION_TAG : String.Empty;

            ue.LoadDockerFile(DockerFilePath, DockerImageName, UEAppName, DockerPortMapping, CommandToExecute, WorkingDir, sessiontag);

            Environment.Exit(0);
        }
    }
}
