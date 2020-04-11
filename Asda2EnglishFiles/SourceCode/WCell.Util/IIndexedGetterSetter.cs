namespace WCell.Util
{
    /// <summary>
    /// The IPropertyAccessor interface defines a property
    /// accessor.
    /// </summary>
    public interface IIndexedGetterSetter
    {
        /// <summary>
        /// </summary>
        /// <returns>Value.</returns>
        object Get(object key, int index);

        /// <summary>
        /// </summary>
        /// <param name="value">Value.</param>
        void Set(object key, int index, object value);
    }
}