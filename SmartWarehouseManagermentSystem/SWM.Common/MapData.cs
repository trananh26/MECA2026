using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
    class MapData
    {
    }
    
    public class Node
    {
        public string ID { get; set; }
        //public string TYPE { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool show { get; set; }
    }

    public class Link
    {
        public string ID { get; set; }
        public string Source { get; set; }
        public string Dest { get; set; }
        public string Distance { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
    }

    public class PathFollowTurnNode
    {
        public Link TurnPath { get; set; }
        public List<Link> HorPath { get; set; }
    }
}
