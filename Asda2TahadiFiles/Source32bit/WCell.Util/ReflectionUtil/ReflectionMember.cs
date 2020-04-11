namespace WCell.Util.ReflectionUtil
{
  public class ReflectionMember
  {
    public readonly string FullName;

    public ReflectionMember(string fullName)
    {
      FullName = fullName;
    }

    public object[] Arguments { get; protected set; }
  }
}