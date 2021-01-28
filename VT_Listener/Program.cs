// This server will listen to the telemetry data from the VT devices on the fleets. 
// Raw data is parsed and stored in to the SQL Server Database for the external app services
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace VT_Listener
{
    public static class  Program
    {
        static double TcpConnectionTimeout = Convert.ToDouble(ConfigurationSettings.AppSettings["TcpConnectionTimeout"].ToString());

        static void Main(string[] args)
        {
            int port = Convert.ToInt16(ConfigurationSettings.AppSettings["port"].ToString());
            TcpListener server = new TcpListener(port);
            server.Start();
            while (true)
            {
                Console.Write("Waiting for a connection(with db)...-- ");
                WriteLog("Waiting for a connection(with db)...-- ");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("new client connected (" + client.Client.LocalEndPoint.ToString() + ")");
                WriteLog("new client connected");
                Task t = new Task(n => HandleClient((object)n), client);
                t.Start();
                WriteLog("HandleClient() Started.");
                t.Wait();
                WriteLog("HandleClient() Wait ing to complete...");
                //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), client);//or use Task if 4.0 or new Thread...
            }
        }

        private static void HandleClient(object tcpClient)
        {
            try
            {
                WriteLog("HandleClient() Entered");
                TcpClient client = (TcpClient)tcpClient;
                if(client.Connected)
                {
                    WriteLog("tcpClient created");
                    Console.Write("tcpClient created");
                    Byte[] bytes = new Byte[2048];
                    String data = null;
                    int i;

                    NetworkStream stream = client.GetStream();
                    WriteLog("Network Stream Created");
                    Console.WriteLine("Network Stream Created");
                    stream.ReadTimeout = 10000;
                    if(stream.CanRead)
                    {
                        Console.WriteLine("Reading stream..");
                        i = stream.Read(bytes, 0, bytes.Length);
                        Console.WriteLine("Stream read..");
                    }
                    else
                    {
                        i = 0;
                        WriteLog("Stream can't be read...");
                        Console.WriteLine("Stream can't be read..");
                    }
                    
                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    Console.Write(data);
                    WriteLog("Date read from NetworkStream :" + data + "\n" + "Processdata calling..");

                    //Byte[] dat = new Byte[256];
                    //dat = Encoding.ASCII.GetBytes(data.Length.ToString() + " Bytes Data Received.");
                    //stream.Write(dat, 0, dat.Length);

                    processdata(data);
                    WriteLog("Processdata Completed");
                    client.Close();
                    WriteLog("Client Closed...");
                    Console.WriteLine(data);
                }
                else
                {
                    client.Close();
                    WriteLog("tcpClient closed with unknown reason.");
                }
                
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            
        }

        private static void processdata(string data)
        {
            try 
            {
                Console.WriteLine("Entering processdata..");
                Console.WriteLine("Data: "  + data);
                string[] dataArr = data.Split('#');
                for (int i = 0; i <= dataArr.Length - 1; i++)
                {
                    if (dataArr[i].Trim().Length == 0)
                        break;
                    data = dataArr[i] + "#";
                    data = data.Replace("\r\n","");
                    if (data.StartsWith("$") && data.EndsWith("#"))
                    {
                        Console.WriteLine("Valid Msg..");
                        data = data.Substring(1, data.Length - 2);
                        string[] dataval = data.Split(',');
                        if (dataval.Length >= 25)
                        {

                            Console.WriteLine("Framing Query..");
                            DAL.CommonClass CC = new DAL.CommonClass();
                            string strsql = "";
                            string gpsvalid = dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_gpsvalidity"])];
                            if (dataval[0] == "WTGPS")
                            {
                                strsql += "Insert into SAFE_TRANS_VECHICLETRANSACTION (";
                                strsql += "[VTR_VENDORID],";
                                strsql += "[VTR_VEHICLEID],";
                                strsql += "[VTR_DATETIME],";
                                strsql += "[VTR_LATITUDE],";
                                strsql += "[VTR_LONGLATITUDE],";
                                strsql += "[VTR_ALTITUDE],";
                                strsql += "[VTR_SPEED],";
                                strsql += "[VTR_COURSE],";
                                strsql += "[VTR_ODOMETER],";
                                strsql += "[VTR_GPSMOVESTATUS],";
                                strsql += "[VTR_IGNITION],";
                                strsql += "[VTR_INPUT1],";
                                strsql += "[VTR_INPUT2],";
                                strsql += "[VTR_DINPUT1],";
                                strsql += "[VTR_DINPUT2],";
                                strsql += "[VTR_OUTPUT1],";
                                strsql += "[VTR_OUTPUT2],";
                                strsql += "[VTR_UPDDATETIME],";
                                strsql += "[VTR_GPSVALIDITY] ";
                                strsql += ") values (";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_vendorid"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_vehicleid"])] + "', ";
                                string datetime = dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_datetime"])];
                                //string[] datestr = dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_date"])].Split('.');
                                //string[] timestr = dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_time"])].Split(':');
                                int date = 0, month = 0, year = 0, hour = 0, min = 0, sec = 0;
                                //if (datestr.Length == 3)
                                //{
                                //    date = Convert.ToInt16(datestr[0]);
                                //    month = Convert.ToInt16(datestr[1]);
                                //    year = Convert.ToInt16(datestr[2]);
                                //}
                                //if (timestr.Length == 3)
                                //{
                                //    hour = Convert.ToInt16(timestr[0]);
                                //    min = Convert.ToInt16(timestr[1]);
                                //    sec = Convert.ToInt16(timestr[2]);
                                //}
                                year = Convert.ToInt16(datetime.Substring(0,4));
                                month = Convert.ToInt16(datetime.Substring(4, 2));
                                date = Convert.ToInt16(datetime.Substring(6, 2));
                                hour = Convert.ToInt16(datetime.Substring(8, 2));
                                min = Convert.ToInt16(datetime.Substring(10, 2));
                                sec = Convert.ToInt16(datetime.Substring(12, 2));

                                DateTime dt = new DateTime(year, month, date, hour, min, sec);
                                strsql += "'" + dt.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_latitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_longitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_altitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_speed"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_Course"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_odometer"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_gpsmovestatusr"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_ignition"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_ainput1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_ainput2"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_dinput1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_dinput2"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_output1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_output2"])] + "', ";
                                strsql += "'" + DateTime.Now.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WM_gpsvalidity"])] + "' ";
                                strsql += ")";
                            }
                            else if (IsNumeric(dataval[0]))
                            {
                                strsql += "Insert into SAFE_TRANS_VECHICLETRANSACTION (";
                                strsql += "[VTR_VEHICLEID],";
                                strsql += "[VTR_DATETIME],";
                                strsql += "[VTR_LATITUDE],";
                                strsql += "[VTR_LONGLATITUDE],";
                                strsql += "[VTR_SPEED],";
                                strsql += "[VTR_ODOMETER],";
                                strsql += "[VTR_DIRECTION],";
                                strsql += "[VTR_IGNITION],";
                                strsql += "[VTR_INPUT1],";
                                strsql += "[VTR_INPUT2],";
                                strsql += "[VTR_IMMOBILIZER],";
                                strsql += "[VTR_UPDDATETIME],";
                                strsql += "[VTR_GPSVALIDITY] ";
                                strsql += ") values (";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_vehicleid"])] + "', ";
                                int date = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_date"])]);
                                int month = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_mon"])]);
                                int year = 2000 + Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_year"])]);
                                int hour = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_hour"])]);
                                int min = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_min"])]);
                                int sec = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_sec"])]);
                                DateTime dt = new DateTime(year, month, date, hour, min, sec);
                                strsql += "'" + dt.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_latitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_longitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_speed"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_odometer"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_direction"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_ignition"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_input1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_input2"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_immobilizer"])] + "', ";
                                strsql += "'" + DateTime.Now.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_gpsvalidity"])] + "' ";
                                strsql += ")";
                            }


                            //Console.WriteLine(strsql);
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = strsql;
                            Console.WriteLine("Executing Query\n");
                            Console.Write(strsql);
                            cmd.CommandTimeout = 3000;
                            bool result = CC.ExecuteCmdObj(cmd);
                            Console.WriteLine("\nResult : " + result);
                            if (result)
                            { Console.WriteLine("records updated"); }
                            else
                            { Console.WriteLine("error occured"); }
                        }

                        if (dataval.Length == 19)
                        {

                            Console.WriteLine("Framing Query..");
                            DAL.CommonClass CC = new DAL.CommonClass();
                            string strsql = "";
                            if (dataval[0] == "WTGPS" || dataval[0] == "EIGPS")
                            {
                                strsql += "Insert into SAFE_TRANS_VECHICLETRANSACTION (";
                                strsql += "[VTR_VENDORID],";
                                strsql += "[VTR_VEHICLEID],";
                                strsql += "[VTR_DATETIME],";
                                strsql += "[VTR_LATITUDE],";
                                strsql += "[VTR_LONGLATITUDE],";
                                strsql += "[VTR_ALTITUDE],";
                                strsql += "[VTR_SPEED],";
                                strsql += "[VTR_COURSE],";
                                strsql += "[VTR_ODOMETER],";
                                strsql += "[VTR_GPSMOVESTATUS],";
                                strsql += "[VTR_IGNITION],";
                                strsql += "[VTR_INPUT1],";
                                strsql += "[VTR_INPUT2],";
                                strsql += "[VTR_DINPUT1],";
                                strsql += "[VTR_DINPUT2],";
                                strsql += "[VTR_OUTPUT1],";
                                strsql += "[VTR_OUTPUT2],";
                                strsql += "[VTR_UPDDATETIME],";
                                strsql += "[VTR_GPSVALIDITY] ";
                                strsql += ") values (";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_vendorid"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_vehicleid"])] + "', ";
                                string[] datestr = dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_date"])].Split('.');
                                string[] timestr = dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_time"])].Split(':');
                                int date = 0, month = 0, year = 0, hour = 0, min = 0, sec = 0;
                                if (datestr.Length == 3)
                                {
                                    date = Convert.ToInt16(datestr[0]);
                                    month = Convert.ToInt16(datestr[1]);
                                    year = Convert.ToInt16(datestr[2]);
                                }
                                if (timestr.Length == 3)
                                {
                                    hour = Convert.ToInt16(timestr[0]);
                                    min = Convert.ToInt16(timestr[1]);
                                    sec = Convert.ToInt16(timestr[2]);
                                }
                                DateTime dt = new DateTime(year, month, date, hour, min, sec);
                                strsql += "'" + dt.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_latitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_longitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_altitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_speed"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_Course"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_odometer"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_gpsmovestatusr"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_ignition"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_ainput1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_ainput2"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_dinput1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_dinput2"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_output1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_output2"])] + "', ";
                                strsql += "'" + DateTime.Now.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["WMO_gpsvalidity"])] + "' ";
                                strsql += ")";
                            }
                            else if (IsNumeric(dataval[0]))
                            {
                                strsql += "Insert into SAFE_TRANS_VECHICLETRANSACTION (";
                                strsql += "[VTR_VEHICLEID],";
                                strsql += "[VTR_DATETIME],";
                                strsql += "[VTR_LATITUDE],";
                                strsql += "[VTR_LONGLATITUDE],";
                                strsql += "[VTR_SPEED],";
                                strsql += "[VTR_ODOMETER],";
                                strsql += "[VTR_DIRECTION],";
                                strsql += "[VTR_IGNITION],";
                                strsql += "[VTR_INPUT1],";
                                strsql += "[VTR_INPUT2],";
                                strsql += "[VTR_IMMOBILIZER],";
                                strsql += "[VTR_UPDDATETIME],";
                                strsql += "[VTR_GPSVALIDITY] ";
                                strsql += ") values (";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_vehicleid"])] + "', ";
                                int date = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_date"])]);
                                int month = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_mon"])]);
                                int year = 2000 + Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_year"])]);
                                int hour = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_hour"])]);
                                int min = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_min"])]);
                                int sec = Convert.ToInt16(dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_sec"])]);
                                DateTime dt = new DateTime(year, month, date, hour, min, sec);
                                strsql += "'" + dt.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_latitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_longitude"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_speed"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_odometer"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_direction"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_ignition"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_input1"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_input2"])] + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_immobilizer"])] + "', ";
                                strsql += "'" + DateTime.Now.ToString() + "', ";
                                strsql += "'" + dataval[Convert.ToInt16(ConfigurationSettings.AppSettings["VT_gpsvalidity"])] + "' ";
                                strsql += ")";
                            }


                            //Console.WriteLine(strsql);
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = strsql;
                            Console.WriteLine("Executing Query\n");
                            Console.Write(strsql);
                            cmd.CommandTimeout = 180;
                            bool result = CC.ExecuteCmdObj(cmd);
                            Console.WriteLine("\nResult : " + result);
                            if (result)
                            { Console.WriteLine("records updated"); }
                            else
                            { Console.WriteLine("error occured"); }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Data :" + data + " \nInvalid Data Format..");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private static void processeidata(string data)
        {
            try
            {
                Console.WriteLine("Entering processdata..");
                string[] dataArr = data.Split('#');
                for (int i = 0; i <= dataArr.Length - 1; i++)
                {
                    if (dataArr[i].Trim().Length == 0)
                        break;
                    data = dataArr[i] + "#";
                    if (data.StartsWith("1 "))
                    {
                        Console.WriteLine("Valid Msg..");
                        data = data.Substring(1, data.Length - 2);
                        string[] dataval = data.Split(' ');
                        if (dataval.Length >= 23)
                        {

                            Console.WriteLine("Framing Query..");
                            DAL.CommonClass CC = new DAL.CommonClass();
                            string strsql = "";
                            strsql += "Insert into [Transaction] (";
                            strsql += "[IMEI],";
                            strsql += "[Cardnumber],";
                            strsql += "[Date_Time],";
                            strsql += "[Latitude],";
                            strsql += "[Longitide],";
                            strsql += "[Remarks]";
                            
                            strsql += ") values (";
                            strsql += "'" + dataval[22] + "', ";
                            strsql += "'" + dataval[2] + "', ";
                            string[] datestr = dataval[5].Split('/');
                            string[] timestr = dataval[6].Split(':');
                            int date = 0, month = 0, year = 0, hour = 0, min = 0, sec = 0;
                            if (datestr.Length == 3)
                            {
                                date = Convert.ToInt16(datestr[0]);
                                month = Convert.ToInt16(datestr[1]);
                                year = Convert.ToInt16(datestr[2]);
                            }
                            if (timestr.Length == 3)
                            {
                                hour = Convert.ToInt16(timestr[0]);
                                min = Convert.ToInt16(timestr[1]);
                                sec = Convert.ToInt16(timestr[2]);
                            }
                            string date_time = month + "/" + date + "/" + year + " " + dataval[6];
                            DateTime dt = new DateTime(year, month, date, hour, min, sec);
                            strsql += "'" + date_time + "', ";
                            string lat = Convert.ToString(DegreetoDecimal(dataval[7]));
                            string lon = Convert.ToString(DegreetoDecimal(dataval[15]));

                            strsql += "'" + lat + "', ";
                            strsql += "'" + lon + "', ";
                            strsql += "'' ";
                            strsql += ")";


                            //Console.WriteLine(strsql);
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = strsql;
                            Console.WriteLine("Executing Query\n");
                            Console.Write(strsql);
                            cmd.CommandTimeout = 180;
                            bool result = CC.ExecuteCmdObj(cmd);
                            Console.WriteLine("\nResult : " + result);
                            if (result)
                            { Console.WriteLine("records updated"); }
                            else
                            { Console.WriteLine("error occured"); }
                        }

                    }
                    else
                    {
                        Console.WriteLine("Data :" + data + " \nInvalid Data Format..");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static decimal DegreetoDecimal(string latorlong)
        {
            decimal deg = Convert.ToDecimal(latorlong.Split('.')[0]);
            decimal min1 = Convert.ToDecimal(deg.ToString().Substring(deg.ToString().Length - 2, 2));
            deg = Convert.ToDecimal(deg.ToString().Substring(0, deg.ToString().Length - 2));
            decimal min2 = Convert.ToDecimal(min1.ToString() + "." + latorlong.Split('.')[1].ToString());

            deg = Math.Round(deg + (min2 / 60), 8);
            return deg;
        }

        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }

        private static void WriteLog(string str)
        {
            try
            {
                if (!Directory.Exists(Application.StartupPath + "\\Log\\"))
                {
                    Directory.CreateDirectory(Application.StartupPath + "\\Log\\");
                }
                string FileName = Application.StartupPath + "\\Log\\ReaderLog_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + ".txt";
                if (!System.IO.File.Exists(FileName))
                {
                    FileStream fs = System.IO.File.Create(FileName);
                    fs.Close();
                    fs = null;
                }
                StreamWriter sw = new StreamWriter(FileName, true);
                sw.WriteLine("[" + DateTime.Now + "] ==>  " + str);
                sw.Close();
                sw = null;
            }
            catch (Exception ex)
            {

            }
        }

        private static TcpClient GetClient(string IP, int port)
        {
            //IP = "192.168.1.116";
            //port = 1001;
            var timeout = TimeSpan.FromMilliseconds(TcpConnectionTimeout);
            var client = new TcpClient();
            bool ConnectStatus = true;
            try
            {
                if (!client.ConnectAsync(IP, port).Wait(timeout))
                {
                    // timed out
                    ConnectStatus = false;
                }
                else
                {
                    return client;
                    //client.Close();
                    //client = null;
                }

            }
            catch (Exception ex)
            {
                ConnectStatus = false;
                client.Close();
                client = null;
                return client;
            }

            client.Close();
            client = null;
            return client;
        }
    }

    class ClientContext
    {
        public TcpClient Client;
        public Stream Stream;
        public byte[] Buffer = new byte[4];
        public MemoryStream Message = new MemoryStream();
    }
}
