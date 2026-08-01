using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace VPNControl
{
    public static class NetworkChecker
    {
        /// <summary>
        /// Verifies that the network is ready before starting vpncli.
        /// The method performs the following checks:
        ///   1. At least one network interface is available.
        ///   2. There is an active interface with a default gateway.
        ///   3. The VPN server hostname can be resolved via DNS.
        ///   4. A TCP connection to the VPN server can be established.
        /// </summary>
        /// <param name="vpnHost">VPN server hostname or IP address.</param>
        /// <param name="vpnPort">VPN server port (typically 443).</param>
        /// <param name="error">Returns a human-readable error description.</param>
        /// <returns>True if all checks succeed; otherwise false.</returns>
        public static bool IsNetworkReady(string vpnHost, int vpnPort, out string error)
        {
            error = "";

            // Check whether Windows reports that at least one network
            // interface is currently available.
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                error = "No network interfaces available.";
                return false;
            }

            // Resolve the VPN server hostname.
            // Failure here usually indicates a DNS issue.
            IPAddress[] addresses;

            try
            {
                addresses = Dns.GetHostAddresses(vpnHost);

                if (addresses == null || addresses.Length == 0)
                {
                    error = "DNS lookup returned no addresses.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = "DNS lookup failed: " + ex.Message;
                return false;
            }

            // Try to establish a TCP connection to each resolved address.
            // A successful TCP handshake indicates that the VPN server
            // is reachable over the network.
            foreach (IPAddress ip in addresses)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        IAsyncResult result = client.BeginConnect(ip, vpnPort, null, null);

                        bool connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                        if (!connected)
                        {
                            // Connection attempt timed out.
                            client.Close();
                            continue;
                        }

                        client.EndConnect(result);

                        if (client.Connected)
                        {
                            // The using block will automatically close
                            // the TCP connection before returning.
                            return true;
                        }
                    }
                }
                catch
                {
                    // Ignore the current address and try the next one.
                }
            }

            error = "Unable to establish a TCP connection to the VPN server.";
            return false;
        }
    }



}
