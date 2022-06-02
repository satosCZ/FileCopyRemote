using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyRemote
{
    public static class NetworkShare
    {
        public static string ConnectToShare(string url, string username, string password)
        {
            NETRESOURCE nr = new NETRESOURCE();
            nr.dwType = RESOURCETYPE.RESOURCETYPE_DISK;
            nr.lpRemoteName = url;

            int ret = WNetUserConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);
            if (ret == NO_ERROR)
        }
    }
}
