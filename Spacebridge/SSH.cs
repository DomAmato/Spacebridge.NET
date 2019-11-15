using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Spacebridge
{
    class SSH
    {
        private static readonly byte[] hologram_fingerprint = new byte[] {
        0xc7, 0xd2, 0x6c, 0xe2, 0x57, 0x50, 0x49, 0xbf,
        0x3c, 0x23, 0x73, 0xc5, 0xcc, 0x39, 0x48, 0xa3 };
        private static readonly string tunnel_server = "tunnel.hologram.io";
        private static readonly int tunnel_port = 999;
        private static readonly List<PrivateKeyFile> spacebridge_key = new List<PrivateKeyFile>();
        private static readonly Dictionary<int, Tuple<ForwardedPortLocal, SshClient>> forwarded_ports = new Dictionary<int, Tuple<ForwardedPortLocal, SshClient>>();

        public static void CreateRSAKey(byte[] publicKey, byte[] privateKey)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram");
            Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "/spacebridge.key", privateKey);
            File.WriteAllBytes(path + "/spacebridge.key.pub", publicKey);

            spacebridge_key.Add(new PrivateKeyFile(path + "/spacebridge.key"));
            //using (var rsa = new RSACryptoServiceProvider(2048))
            //{
            //    try
            //    {

            //        // Do something with the key...
            //        // Encrypt, export, etc.
            //    }
            //    finally
            //    {
            //        rsa.PersistKeyInCsp = false;
            //    }
            //}
        }

        public static void CreateDSSKey()
        {
            using var dsa = new DSACryptoServiceProvider(2048);
            try
            {
                // Do something with the key...
                // Encrypt, export, etc.
            }
            finally
            {
                dsa.PersistKeyInCsp = false;
            }
        }

        public static bool BeginForwarding(int linkId, int local_port, int remote_port)
        {
            if (forwarded_ports.ContainsKey(local_port))
            {
                System.Diagnostics.Debug.WriteLine("Local port is already forwarding, use a different port");
                return false;
            }
            if (spacebridge_key.Count == 0)
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram");
                spacebridge_key.Add(new PrivateKeyFile(path + "/spacebridge.key"));
            }
            using var client = new SshClient(tunnel_server, tunnel_port, "htunnel", spacebridge_key.ToArray());
            client.HostKeyReceived += (sender, e) =>
            {
                if (hologram_fingerprint.Length == e.FingerPrint.Length)
                {
                    for (var i = 0; i < hologram_fingerprint.Length; i++)
                    {
                        if (hologram_fingerprint[i] != e.FingerPrint[i])
                        {
                            e.CanTrust = false;
                            break;
                        }
                    }
                }
                else
                {
                    e.CanTrust = false;
                }
            };
            try
            {
                client.Connect();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }

            var port = new ForwardedPortLocal("localhost", (uint)local_port, "link" + linkId, (uint)remote_port);
            client.AddForwardedPort(port);

            port.Exception += delegate (object sender, ExceptionEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
            };
            port.Start();
            forwarded_ports.Add(local_port, new Tuple<ForwardedPortLocal, SshClient>(port, client));
            return true;
        }

        public static void StopForwarding(int local_port)
        {
            var forwardingClient = forwarded_ports[local_port];
            if (forwardingClient != null)
            {
                forwardingClient.Item1.Stop();
                forwardingClient.Item2.Disconnect();
            }
        }
    }
}
