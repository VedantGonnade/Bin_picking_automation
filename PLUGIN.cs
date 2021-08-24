using CPRogPluginFramework; // Dieser Namespace wird als Referenz auf der Registerkarte "Lösung" (Solution) hinzugefügt.
using System;
using System.Collections.Generic;

namespace PluginSimple
{
    class PLUGIN : ICPRogPluginCmd // Klasse von CPRogPluginFramework wird als Schnittstelle verwendet.
    {
        //Dies zeigt die Fehlermeldungen oder andere Meldungen.
        public string logMsg = "";

        //Ordner, in dem sich das Plugin befinden soll.
        string PluginDirPath = @"C:\CPRog\Data\Plugins\";

        //Eine neue Liste der TCPIP-Klassen wird erstellt und in einer variablen "camlist" gespeichert.
        public List<TCPIP> camList = new List<TCPIP>();

        //Nur eine Ganzzahlvariable und eine Boolesche Variable, die im Code verwendet werden.
        public int nrOfCameras = 1;
        bool flagPluginInited;


        // Diese Methode gibt den Namen des Plugins an.
        public string PluginName
        {
            get
            {
                // Was auch immer wir hier in (return"") schreiben, sollte der Name des Plugins sein. 
                // In useremfall der Name des Plugins ist PluginSimple. 
                return "PluginSimple";
                
            }
        }

        // Diese Methode muss immer so bleiben. Nicht wichtig zu ändern.
        public bool MotionTypeJoints
        {
            get
            {
                return false;
            }
        }

        // Diese Methode muss immer so bleiben. Nicht wichtig zu ändern.
        public int PluginInterfaceVersion
        {
            get
            {
                return 1;
            }
        }

        // In dieser Methode kann man Beschreibungen über das Plugin schreiben.
        public void GetPluginDescription(
           ref string Version,
           ref string DescriptionShort,
           ref string Description)
        {
            Version = "1";
            DescriptionShort = "Interface to IFM O3D smart cameras";
            Description = "Cameras are connected via TCP/IP and provide position and rotation of the workpiece.";
        }

        // Man kann über die Herstellung des Plugins in dieser Methode schreiben
        public void GetPluginManufacturer(
          ref string Company,
          ref string website,
          ref string supportMail)
        {
            Company = "Nolta GmbH";
            website = "";
            supportMail = "";
        }


        // Diese Methode gibt dem Roboter die x-, y- und z-Koordinaten.
        public int GetTargetPosition(int camNr, ref int resClass, ref float[] posCart, ref float[] oriEuler)
        {


            for (int index = 0; index < nrOfCameras; index++)
            {
                // resClass ist unnoetig. Ich habe das gleiche kopieriet was ich auf dem Sample code gefunden habe. 
                resClass = 3;

                // Anfangen der Verbindung.
                this.camList[index].Connect();

                // Bekommen wir die coordinaten.
                posCart[0] = float.Parse(this.camList[index].xcoordinateZero.ToString());

                posCart[1] = float.Parse(this.camList[index].ycoordinateZero.ToString());

                posCart[2] = float.Parse(this.camList[index].zCoordinateMinn.ToString());

                if ((double)oriEuler[0] > 180.0)
                    oriEuler[0] -= 360f;
                if ((double)oriEuler[0] < -180.0)
                    oriEuler[0] += 360f;

            }

            
            for (int i = 0; i < nrOfCameras; i++)
            {
                this.camList[i].Disconnect();
            }

       
            return 1;
                           
        }


        // Diese Methode sollte es einfach so lassen.
        public int UpdatePosition(robotStatus robState, TimeSpan diffTime, double ovr, ref double[] joints, ref float[] positionMatrix, ref double[] orientationEuler, bool[] digitalIn, ref bool[] digitalOut)
        {
            return 0;
        }


        // Diese Methode wird verwendet, um das Plugin zu starten, 
        // alle Variablen zu initialisieren und eine TCPIP-Klasse in eine Liste aufzunehmen.
        public int Initialize()
        {

            if (this.flagPluginInited)
            {
                this.logMsg = "Camera already initialized...";
                return 0;
            }
            this.flagPluginInited = true;

            // Diese Meldung wird in der CPROG-Software angezeigt.
            this.logMsg = "Initializing the camera"; 

            // In TCPIP.cs erwähnte Variablen werden initialisiert.
            this.camList.Add(new TCPIP()); 


            this.flagPluginInited = false;


            return 0;
        }


        // Diese Methode legt den Ort fest, an dem das Plugin bleiben soll.
        public void SetPluginDirectoryPath(string path)
        {
            this.PluginDirPath = path;
        }


        // nicht wichtig, einfach leer lassen.
        public void ShowGUI()
        {
            ;
        }


        // Diese Methode zeigt die Meldungen in der CPRog-Software an.
        public string LogMessage()
        {
            string str = this.logMsg + this.camList[0].logMsg;
            this.logMsg = "";
            this.camList[0].logMsg = "";
            return str;
        }


        

        

       
         
       



        

        
    }
}
