using System;

namespace WCell.RealmServer.Global
{
  public class RealmMessage2<T1, T2> : IRealmMessage
  {
    /// <summary>Default constructor.</summary>
    public RealmMessage2()
    {
    }

    /// <summary>Constructs a message with the specific callback.</summary>
    /// <param name="callback">the callback to invoke when the message is executed</param>
    public RealmMessage2(Action<T1, T2> callback)
      : this(callback, RealmMessageBoundary.Global)
    {
    }

    /// <summary>Constructs a message with the specific callback.</summary>
    /// <param name="callback">the callback to invoke when the message is executed</param>
    public RealmMessage2(Action<T1, T2> callback, RealmMessageBoundary boundary)
    {
      Callback = callback;
      Boundary = boundary;
    }

    /// <summary>
    /// Constructs a message with the specific callback and input parameters.
    /// </summary>
    /// <param name="callback">the callback to invoke when the message is executed</param>
    /// <param name="param1">the first input parameter</param>
    /// <param name="param2">the second input parameter</param>
    public RealmMessage2(T1 param1, T2 param2, Action<T1, T2> callback)
      : this(param1, param2, callback, RealmMessageBoundary.Global)
    {
    }

    /// <summary>
    /// Constructs a message with the specific callback and input parameters.
    /// </summary>
    /// <param name="callback">the callback to invoke when the message is executed</param>
    /// <param name="param1">the first input parameter</param>
    /// <param name="param2">the second input parameter</param>
    public RealmMessage2(T1 param1, T2 param2, Action<T1, T2> callback, RealmMessageBoundary boundary)
    {
      Callback = callback;
      Boundary = boundary;
      Parameter1 = param1;
      Parameter2 = param2;
    }

    /// <summary>
    /// Constructs a message with the specific callback and input parameters.
    /// </summary>
    /// <param name="callback">the callback to invoke when the message is executed</param>
    /// <param name="param1">the first input parameter</param>
    /// <param name="param2">the second input parameter</param>
    public RealmMessage2(T1 param1, T2 param2)
    {
      Parameter1 = param1;
      Parameter2 = param2;
    }

    public RealmMessageBoundary Boundary { get; private set; }

    /// <summary>
    /// The callback that is called when the message is executed.
    /// </summary>
    public Action<T1, T2> Callback { get; set; }

    /// <summary>The first input parameter.</summary>
    public T1 Parameter1 { get; set; }

    /// <summary>The second input parameter.</summary>
    public T2 Parameter2 { get; set; }

    /// <summary>
    /// Executes the message, calling any callbacks that are bound, passing the given input parameters.
    /// </summary>
    public virtual void Execute()
    {
      Action<T1, T2> callback = Callback;
      if(callback == null)
        return;
      callback(Parameter1, Parameter2);
    }

    public static explicit operator RealmMessage2<T1, T2>(Action<T1, T2> dele)
    {
      return new RealmMessage2<T1, T2>(dele);
    }
  }
}