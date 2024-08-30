using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace StreamViewer
{
    public class StreamManager
    {
        public List<StreamModel> Streams { get; private set; } = new List<StreamModel>();

        public void LoadStreamsFromFile(string filePath)
        {
            Streams.Clear();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    Streams.Add(new StreamModel
                    {
                        Url = parts[0].Trim(),
                        Title = parts[1].Trim(),
                        Type = DetermineStreamType(parts[0].Trim())
                    });
                }
            }
        }

        private StreamType DetermineStreamType(string url)
        {
            return url.StartsWith("rtsp://") ? StreamType.RTSP : StreamType.Web;
        }
    }

    public class StreamModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public StreamType Type { get; set; }
    }

    public enum StreamType
    {
        Web,
        RTSP
    }
}