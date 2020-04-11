using WCell.RealmServer.Paths;

namespace WCell.RealmServer.Entities
{
    internal class TransportPathTempVertex
    {
        public PathVertex Vertex;
        public float DistFromFirstStop;
        public float DistToLastStop;
        public float MoveTimeFromFirstStop;
        public float MoveTimeToLastStop;
        public float MoveTimeFromPrevious;

        public TransportPathTempVertex(float fromFirstStop, float toLastStop, float fromPrevious, PathVertex v)
        {
            this.MoveTimeFromFirstStop = fromFirstStop;
            this.MoveTimeToLastStop = toLastStop;
            this.MoveTimeFromPrevious = fromPrevious;
            this.Vertex = v;
        }
    }
}