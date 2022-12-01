using System;

namespace Vltava.Core.Framework
{
    /// <summary>
    /// This structure to indicates no value (to be used with generics). We need to resort to this because System.Void does not work.
    /// </summary>
    public class None
    {
        public static None Instance
        {
            get { return new None(); }
        }

        public static Result<None> True()
        {
            return Result<None>.True(None.Instance);
        }

        public static Result<None> False(Exception ex)
        {
            return Result<None>.False(ex);
        }
    }
}