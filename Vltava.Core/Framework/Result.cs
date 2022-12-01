using System;
using System.Collections.Generic;

namespace Vltava.Core.Framework
{
    /// <summary>
    /// This is a simple return value structure to make passing return value easier.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Result<T>
    {
        public Result(bool ist, T val)
        {
            _isTrue = ist;
            Value = val;
            _exceptionObject = null;
            _message = null;
        }

        public Result(bool ist, T val, string msg)
            : this(ist, val)
        {
            Message = msg;
        }

        /// <summary>
        /// Hold the value of the return value
        /// </summary>
        public T Value;

        bool _isTrue;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is true.
        /// </summary>
        /// <value><c>true</c> if this instance is true; otherwise, <c>false</c>.</value>
        public bool IsTrue
        {
            get { return _isTrue; }
            set { _isTrue = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is false.
        /// </summary>
        /// <value><c>true</c> if this instance is false; otherwise, <c>false</c>.</value>
        public bool IsFalse
        {
            get { return !_isTrue; }

            set { _isTrue = !value; }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private Exception _exceptionObject;
        public Exception ExceptionObject
        {
            get
            {
                //This is deliberate because sometime people do not always pass exception when the false value is set. So 
                //if somebody tries to retrieve this Exception object while the result wasn't set in the first place, we must give them valid 
                //exception object although it won't contains much information about stack trace
                if (_exceptionObject == null && IsFalse)
                    _exceptionObject = new ApplicationException(Message ?? "Exception Message is Null");

                return _exceptionObject;
            }

            set
            {
                _exceptionObject = value;
            }
        }

        /// <summary>
        /// Sets the true value
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetTrue(T val)
        {
            IsTrue = true;
            Value = val;
        }

        public void SetFalse()
        {
            IsTrue = false;
        }

        /// <summary>
        /// Sets the false value by passing exception
        /// </summary>
        /// <param name="e">The e.</param>
        public void SetFalse(Exception e)
        {
            IsTrue = false;
            ExceptionObject = e;
            Message = e.Message + " - source : " + e.Source + " - stack trace : " + e.StackTrace;
        }

        /// <summary>
        /// Return false return value.
        /// </summary>
        /// <returns></returns>
        public static Result<T> False()
        {
            var r = new Result<T>();
            r.SetFalse();

            return r;
        }

        /// <summary>
        /// Return false return value.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public static Result<T> False(Exception e)
        {
            var r = new Result<T>();
            r.SetFalse(e);

            return r;
        }

        public static Result<T> False(string errorMsg)
        {
            var r = new Result<T>();
            r.SetFalse();
            r.Message = errorMsg;

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<T> True(T value)
        {
            var r = new Result<T>();
            r.SetTrue(value);

            return r;
        }

        public static Result<List<T>> True(List<T> values)
        {
            var r = new Result<List<T>>();
            r.SetTrue(values);
            return r;
        }


        /// <summary>
        /// Return false if the object is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Result<T> Conditional(T val)
        {
            return val == null ? Result<T>.False("Object is null") : Result<T>.True(val);
        }

        public static implicit operator T(Result<T> returnValue)
        {
            return returnValue.Value;
        }
    }

}