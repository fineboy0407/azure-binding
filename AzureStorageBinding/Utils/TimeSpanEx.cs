namespace AzureStorageBinding.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class TimeSpanEx
    {
        public static DateTime AfterTimeSpanFromNow(this TimeSpan timeout)
        {
            DateTime end;
            try
            {
                if (timeout == TimeSpan.MaxValue)
                {
                    end = DateTime.MaxValue;
                }
                else
                {
                    end = DateTime.Now + timeout;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // The arithmetic result is out of range. Set timeout to max value.
                end = DateTime.MaxValue;
            }

            return end;
        }
    }
}