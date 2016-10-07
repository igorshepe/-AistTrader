using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class UniqueIdentifier
    {
        private const int length = 10;

        public static string GetUniqueIdentifier()
        {
            string guidResult = string.Empty;

            while (true)
            {
                while (guidResult.Length < length)
                {
                    guidResult += Guid.NewGuid().ToString().GetHashCode().ToString("x");
                }

                if (length > 0 && length <= guidResult.Length)
                {
                    break;
                }
                else
                {
                    guidResult = string.Empty;
                }
            }

            return guidResult.Substring(0, length);
        }
    }
}
