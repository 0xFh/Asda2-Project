namespace WCell.Util
{
    public interface IProducer<T> : IProducer
    {
        /// <summary>Creates a new object of Type T</summary>
        T Produce();
    }
}