namespace WCell.Util
{
    public interface IProducer
    {
        /// <summary>Creates a new object of Type T</summary>
        object Produce();
    }
}