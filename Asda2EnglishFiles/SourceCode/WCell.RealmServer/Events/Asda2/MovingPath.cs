using System.Collections.Generic;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2
{
    public class MovingPath : List<Vector3>
    {
        private readonly int _offset;

        public MovingPath(int offset)
        {
            this._offset = offset;
        }

        public MovingPath Add(int x, int y)
        {
            this.Add(new Vector3((float) (this._offset + x), (float) (this._offset + y)));
            return this;
        }
    }
}