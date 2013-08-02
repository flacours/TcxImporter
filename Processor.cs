using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Linq;
using System.IO;

namespace TcxImporter
{
    internal class Processor
    {
        private Form1 m_form;
        private int m_maxPoints;
        private static XNamespace ns = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";
        public Processor(Form1 form)
        {
            m_form = form;
            m_maxPoints = Properties.Settings.Default.MaxPoints;
        }

        public int MaxPoints { get { return m_maxPoints; } set { m_maxPoints = value; } }

        public DataTable DoConvert(string fileName)
        {
            DataTable dt = CreateDatatable();
            try
            {

                XElement db = XElement.Load(fileName).Element(ns + "Activities");
                var s = db.ToString();
                DateTime prevTimeSecond = DateTime.MinValue;
                double prevDistanceMeter = 0;
                TimeSpan span;
                double totalSeconds;
                List<object[]> points = new List<object[]>();
                foreach (XElement activity in db.Elements(ns + "Activity"))
                {
                    foreach (XElement lap in activity.Elements(ns + "Lap"))
                    {
                        foreach (XElement track in lap.Elements(ns + "Track"))
                        {
                            foreach (XElement trackpoint in track.Elements(ns + "Trackpoint"))
                            {
                                DateTime ts;
                                double lat, lng, altitude, distance, speed;

                                ts = ReadDateTime(trackpoint, "Time");
                                XElement pos = trackpoint.Element(ns + "Position");
                                if (pos != null)
                                {
                                    lat = ReadDouble(pos, "LatitudeDegrees");
                                    lng = ReadDouble(pos, "LongitudeDegrees");
                                    altitude = ReadDouble(trackpoint, "AltitudeMeters");
                                    distance = ReadDouble(trackpoint, "DistanceMeters");
                                    totalSeconds = 0;
                                    speed = 0;
                                    if (prevTimeSecond != DateTime.MinValue)
                                    {
                                        span = ts - prevTimeSecond;
                                        double deltaDistance = distance - prevDistanceMeter;
                                        totalSeconds = span.TotalSeconds;
                                        if(totalSeconds > 0) speed = deltaDistance / span.TotalSeconds * 3600 / 1000;
                                    }
                                    prevTimeSecond = ts;
                                    prevDistanceMeter = distance;
                                    if(totalSeconds > 0) points.Add(new object[] { ts, lat, lng, altitude, distance, speed });
                                }
                            }
                        }
                    }
                }
                if (points.Count > m_maxPoints)
                {
                    int factor = (int)Math.Ceiling((double)points.Count / m_maxPoints);
                    List<object[]> newpoints = new List<object[]>();
                    int i = 0;
                    foreach (var p in points)
                    {
                        if ((++i % factor) == 0)
                        {
                            newpoints.Add(p);
                        }
                    }
                    points = newpoints;
                }
                foreach (var p in points)
                    dt.Rows.Add(p);
            }
            catch (Exception ex)
            {
                m_form.PrintDiag("Exception : " + ex.Message, DiagType.Error);
            }
            return dt;
        }

        private static double ReadDouble(XElement elem, string name)
        {
            return Convert.ToDouble(elem.Element(ns + name).Value);
        }
        private static DateTime ReadDateTime(XElement elem, string name)
        {
            return Convert.ToDateTime(elem.Element(ns + name).Value);
        }


        private DataTable CreateDatatable()
        {
            DataTable dt = new DataTable("Trackpoint");
            dt.Columns.Add("Time", typeof(DateTime));
            dt.Columns.Add("Latitude", typeof(double));
            dt.Columns.Add("Longitude", typeof(double));
            dt.Columns.Add("AltitudeMeters", typeof(double));
            dt.Columns.Add("DistanceMeters", typeof(double));
            dt.Columns.Add("Speed (km/h)", typeof(double));
            return dt;
        }


        public void SaveFile(string fileName, DataTable dt)
        {
            try
            {
                
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.WriteLine(String.Format("time,latitude,longitude,altitude_meters,distance_meters,speed_km_h"));
                    //2013-08-01T14:01:20.500
                    foreach (DataRow row in dt.Rows)
                    {
                        sw.WriteLine(String.Format("{0},{1},{2},{3},{4},{5}", row[0], row[1], row[2], row[3], row[4], row[5]));
                    }
                }
                m_form.PrintDiag("Success: " + fileName, DiagType.Success);
            }
            catch (Exception ex)
            {
                m_form.PrintDiag("Exception : " + ex.Message, DiagType.Error);
            }
        }

    }
}
