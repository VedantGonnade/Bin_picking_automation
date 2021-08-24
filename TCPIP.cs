using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PluginSimple
{
    class TCPIP
    {
        // Port Nummer der kamera.
        public int port = 50010;
        public string logMsg = "";

        // IPAddress der kamera.
        public string ipAdress = "169.254.104.24";
        
        // neue TCPCLient heisst clientSocket (unser Computer ist clientSocket).
        public TcpClient clientSocket;

        // Nachfolgend sind die verschiedenen Variablen aufgeführt, die in den Codes verwendet werden.
        public List<double> zcoordinate;
        public double zaxis = 0.0;
        public List<double[]> FinalCoordinates;

        public double xCoordinateMin = 0.0;
        public double yCoordinateMin = 0.0;
        public List<double> xcoordinate;
        public List<double> ycoordinate;

        public double xcoordinateZero = 0.0;
        public double ycoordinateZero = 0.0;
        public double zCoordinateMinn = 0.0;

        public bool connected;


        // Die Konstruktorklasse der TCPIP-Klasse wird verwendet.
        public TCPIP()
        {
            connected = true;
            
        }


        // Die Disconnect() method wird verwendet, nachdem der Roboter die Koordinaten empfangen hat. 
        // man kann das in der GetTargetPosition() Methode in PLUGIN.cs sehen
        public void Disconnect()
        {
            if (clientSocket != null)
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Dispose();
                    clientSocket.Close();
                }
            }
        }


        //Die Methode Connect () initialisiert die wesentliche Variable, 
        //erstellt einen neuen Client, verbindet den Client mit dem Server (die Kamera) und 
        //beginnt mit dem Lesen der Daten von der Kamera.
        public void Connect()
        {
            try
            {
                // Variablen werden initialisiert
                xcoordinateZero = 0.0;
                ycoordinateZero = 0.0;
                zCoordinateMinn = 0.0;

                xcoordinate = new List<double>();
                ycoordinate = new List<double>();
                zcoordinate = new List<double>();
                FinalCoordinates = new List<double[]>();

                // Jetzt ist der variable clientsocket wird ein neuer Client (Computer).
                clientSocket = new TcpClient();
                connected = true;

                logMsg = "Connecting to " + ipAdress + "having port number " + port;

                // Hier versucht der Client (Computer), eine Verbindung mit der Server (Kamera) herzustellen.
                clientSocket.Connect(ipAdress, port);
                logMsg = "Connected to " + ipAdress + "having port number" + port;

                //Hier liest der Client (Computer) die Informationen, die vom Server (Kamera) gesendet wurden.
                //Weitere Informationen zum Lesen finden Sie in der Methode "readData(clientSocket)".
                readData(clientSocket);

            }
            catch (Exception excp)
            {
                this.logMsg = "Cant Connect 1";
                this.logMsg = excp.ToString();

            }
        }


        // Diese Methode sendet Daten an den Server (die Kamera), die die Kamera grundsätzlich anweisen, ein Foto aufzunehmen.
        public async void SendToAll(string leMessage = "")
        {
            if (string.IsNullOrEmpty(leMessage))
            {
                return;
            }
            try
            {
                // Das Byte-Array wird in das ASCII-Format konvertiert.
                byte[] buffMessage = Encoding.ASCII.GetBytes(leMessage);

                // Die BuffMessage wird mit der Method GetStream().WriteAsync() gesendet.
                clientSocket.GetStream().WriteAsync(buffMessage, 0, buffMessage.Length);
                

            }
            catch (Exception excp)
            {
                logMsg = excp.ToString();

            }
        }

        //Diese Methode liest die von der Kamera erhaltenen Daten und gibt uns 'x', 'y' und 'z' Koordinaten.
        public void readData(TcpClient mClient)
        {
            

            try
            {
                
                StreamReader clientStreamReader = new StreamReader(mClient.GetStream());

                // Die erhaltenen Daten werden im Zeichenarray buff gespeichert.
                char[] buff = new char[1024];

                // Dadurch wurde die Anzahl der Zeichen gespeichert, die später im Code verwendet werden.
                int readByCount = 0;
                connected = true;

                // Wenn Sie dies senden (weitere Informationen finden Sie in der Kameradokumentation), 
                //nimmt die Kamera ein Foto auf und sendet die Daten auch an den Client (Computer).
                SendToAll("1234L000000008\r\n1234T?\r\n");

                //connected ist true, while(connected) bedeutet, dass, wenn die obige Bedingung wahr ist, Folgendes passiert.
                while (connected)
                {

                    // Jetzt hat readByCount die Anzahl der Zeichen (integer), die von der Kamera gesendet wurden.
                    readByCount = clientStreamReader.Read(buff, 0, buff.Length);

                    //readByCount > 30 wird verwendet, weil die Kamera eine Zeichenfolge sendet, die nicht erforderlich ist, 
                    //und die Größe dieser bestimmten Zeichenfolge weniger als 30 beträgt. 
                    //Wir möchten nur die Zeichenfolge mit einer Größe von mehr als 30.
                    if (readByCount > 30)
                    {
                        // Die Trimmmethode wird verwendet, um den zusätzlichen Teil der Zeichenfolge zu entfernen, 
                        //und wird dann in der Variablen 'output' gespeichert ;;
                        var output = (new string(buff).TrimEnd('\u0000'));

                        // Eine andere Methode wird verwendet, um die Zeichenfolge zu teilen, und dann wird sie in der Variablen 'output1' gespeichert.
                        var output1 = output.Split(new[] { ";;" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Split(';')).ToArray();

                        // Die Zeichenfolge wird in das JSON-Format konvertiert.
                        JToken jsonParsed = JToken.FromObject(output1);

                        // Die letzte Komponente der Zeichenfolge hat ein Zeilenumbruchzeichen (\ r \ n), das entfernt werden muss. Es wird hier gemacht
                        jsonParsed.Last.Remove();


                        // Die erste Komponente enthält außerdem zusätzliche Informationen, 
                        //die anhand der Dokumentation besser verstanden werden können. Das brauchen wir nicht und es wird hier entfernt.
                        jsonParsed.First.Remove();

                        // Jetzt haben wir ein Array eines Arrays. 
                        //Jedes Array hat 5 Komponenten, x1, y1, x2, y2 und z


                        // Arrayitem in jsonparsed bezeichnet jedes Array im jsonparsed-Array, 
                        //indem berücksichtigt wird, dass jsonparsed ein Array eines Arrays ist.
                        foreach (var arrayItem in jsonParsed)
                        {
                            // arrayItem wird in das double Element konvertiert und in der Variablen innerArray gespeichert. 
                            var innerArray = arrayItem.ToObject<double[]>();

                            // 'FinalCoordinates' ist eine Liste eines Arrays, in dem alle innerarray-Elemente hinzugefügt werden.
                            FinalCoordinates.Add(innerArray);

                            // Das 4. Element (Z-Koordinate) der Liste wird in einem Variable mit dem Namen "zaxis" hinzugefügt.
                            zaxis = innerArray.ElementAt<double>(4);

                            // Dieses 4. Element wird in eine Liste zcoordinate eingefügt.
                            zcoordinate.Add(zaxis);


                            // foreach-loop wird auf zcoordinate ausgeführt, um den Mindestwert in der gesamten Liste zu ermitteln.
                            foreach (var item in zcoordinate)
                            {
                                zCoordinateMinn = zcoordinate.Min();

                            }

                        }

                        // hier für alle innerArray-Arrays, die in der Liste finalcoordinate gespeichert sind,
                        // Das Array, in dem das 4. Element der Mindestwert der z-Koordinate ist, 
                        //nur für dieses bestimmte Array, wird die x- und y-Koordinate ermittelt.

                        foreach (var item in FinalCoordinates)
                        {

                            if (item.ElementAt(4) == zCoordinateMinn)
                            {
                                xCoordinateMin = (item[0] + item[2]) / 2;
                                yCoordinateMin = (item[1] + item[3]) / 2;

                                xcoordinate.Add(xCoordinateMin);
                                ycoordinate.Add(yCoordinateMin);
                                                                                              
                            }

                        }
                        
                        xcoordinateZero = xcoordinate.ElementAt(0);
                        ycoordinateZero = ycoordinate.ElementAt(0);
                        zCoordinateMinn = 1000 * zCoordinateMinn;

                        // Die Convert() methode konvertiert die erhaltenen x, y (die im Pixelformat vorliegen) in Millimeter.
                        Convert();

                        

                        xcoordinate.Clear();
                        ycoordinate.Clear();
                        zcoordinate.Clear();
                        FinalCoordinates.Clear();
                      
                        connected = false;
                                             

                    }
                }
                Array.Clear(buff, 0, buff.Length);
                
                

            }
            catch (Exception ex)
            {
                this.logMsg = "O3D: Could not connect to camera:" + ex.ToString();
            }
        }

        public void Convert()
        {
            xcoordinateZero = (545 / 132) * xcoordinateZero;
            xcoordinateZero = 45 + xcoordinateZero; 
            
            ycoordinateZero = (725 / 176) * ycoordinateZero;
            ycoordinateZero = -370 + ycoordinateZero;

            // Mit der Zconvert() Methode können Sie die mit dem Roboter erzielte Höhe anpassen.
            ZConvert();

            

        }

        public void ZConvert()
        {
            double zCoordinateTop = 510;
            
            double difference = 0.0;
            difference = zCoordinateMinn - zCoordinateTop;
            double zCalibrateValue = 400;
            zCalibrateValue = zCalibrateValue + (difference / 3.5);
            zCoordinateMinn = zCoordinateMinn - zCalibrateValue;
            zCoordinateMinn = 317 - zCoordinateMinn;
        }

    }
}
