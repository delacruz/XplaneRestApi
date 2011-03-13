using System;

namespace XplaneServices
{
    /// <summary>
    /// Generic EventArgs.
    /// </summary>
    [Serializable]
    public class EventArgs<T> : EventArgs
    {
        public new static readonly EventArgs<T> Empty = new EventArgs<T>();

        public T Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public EventArgs(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="EventArgs&lt;T&gt;"/> class from being created.
        /// </summary>
        private EventArgs()
        {
        }
    }
}
