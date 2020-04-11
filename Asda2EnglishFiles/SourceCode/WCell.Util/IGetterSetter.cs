namespace WCell.Util
{
    /// <summary>
    /// </summary>
    public interface IGetterSetter
    {
        /// <summary>
        /// </summary>
        /// <returns>Value.</returns>
        object Get(object key);

        /// <summary>
        /// </summary>
        /// <param name="value">Value.</param>
        void Set(object key, object value);
    }
}