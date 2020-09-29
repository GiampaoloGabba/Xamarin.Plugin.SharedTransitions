namespace Plugin.SharedTransitions.Shared.Utils
{
    /// <summary>
    /// Get an event when a property changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableProperty<T>
    {
        T _value;
        public delegate void ChangeEvent(T data);
        public event ChangeEvent Changed;

        public ObservableProperty(T initialValue)
        {
            _value = initialValue;
        }

        internal void Set(T value)
        {
            if ((value == null && _value != null) ||
                (value != null && !value.Equals(_value)))
            {
                _value = value;
                Changed?.Invoke(_value);
            }
        }

        public T Get()
        {
            return _value;
        }

        public static implicit operator T(ObservableProperty<T> p)
        {
            return p._value;
        }
    }
}
