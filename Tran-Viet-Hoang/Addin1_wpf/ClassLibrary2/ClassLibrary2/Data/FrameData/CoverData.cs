﻿namespace ClassLibrary2.Data.FrameData
{
    public class CoverData
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Side { get; set; }

        public CoverData()
        {
            Side = 50 / 304.8;
        }
    }
}