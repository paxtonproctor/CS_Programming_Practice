using System;
using System.Collections.Generic;
using System.Text;

namespace GraphLine
{
    public class Line
    {
        private Int32 minTrace;
        private Int32 maxTrace;
        private string lineName;
        private System.Drawing.Color lineHue;
        private System.Drawing.Drawing2D.DashStyle lineType;
        private UInt32 lineSize;
        private Boolean graph;

        public Line(Int32 minVal,
                    Int32 maxVal,
                    string name,
                    System.Drawing.Color lineClr,
                    System.Drawing.Drawing2D.DashStyle lineStyl,
                    UInt32 lineWdth,
                    Boolean graphOnOff)
        {
            minTrace = minVal;
            maxTrace = maxVal;
            lineName = name;
            lineHue = lineClr;
            lineType = lineStyl;
            lineSize = lineWdth;
            graph = graphOnOff;
        }

        public Int32 minVal
        {
            get {return minTrace;}
            set {minTrace = value;}
        }

        public Int32 maxVal
        {
            get { return maxTrace; }
            set { maxTrace = value; }
        }

        public UInt32 lineWidth
        {
            get { return lineSize; }
            set { lineSize = value; }
        }

        public System.Drawing.Color lineColor
        {
            get { return lineHue; }
            set { lineHue = value; }
        }

        public System.Drawing.Drawing2D.DashStyle lineStyle
        {
            get { return lineType; }
            set { lineType = value; }
        }

        public string name
        {
            get { return lineName; }
            set { lineName = value; }
        }

        public Boolean showOnGraph
        {
            get { return graph; }
            set { graph = value; }
        }
    }
}
