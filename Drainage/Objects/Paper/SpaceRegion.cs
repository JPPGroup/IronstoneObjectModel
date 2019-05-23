namespace Jpp.Ironstone.Drainage.ObjectModel.Objects.Paper
{
    //MOVE: To Core...
    public class SpaceRegion
    {
        public SpaceRegion RightRegion { get; set; }
        public SpaceRegion BottomRegion { get; set; }
        public int PosY { get; set; }
        public int PosX { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public bool IsOccupied { get; set; }
    }
}
