using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmethystSoftware
{
    public enum ShapeOfPlug
    {
        Circle,
        Rectagular
    }
    public enum DirectionOfPlug
    {
        Horizontal,
        Vertical
    }
    public class PlugInformation
    {
        public ShapeOfPlug Shape { get; set; }
        public DirectionOfPlug Direction { get; set; }
        public double Width { get; set; }
        public XYZ Center { get; set; }
        public double Angle { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double Diameter { get; set; }
        public Level LevelOfPlug { get; set; }
        public FamilyInstance PlugInstance { get; set; }

        public PlugInformation(DirectionOfPlug direction)
        {
            this.Direction = direction;
        }

    }
}