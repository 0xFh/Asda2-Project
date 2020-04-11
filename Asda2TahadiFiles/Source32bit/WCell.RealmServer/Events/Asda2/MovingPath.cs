using System.Collections.Generic;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Events.Asda2
{
  public class MovingPath : List<Vector3>
  {
    private readonly int _offset;

    public MovingPath(int offset)
    {
      _offset = offset;
    }

    public MovingPath Add(int x, int y)
    {
      Add(new Vector3(_offset + x, _offset + y));
      return this;
    }
  }
}