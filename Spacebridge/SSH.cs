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
        private static byte[] hologram_fingerprint = new byte[] {
        0xa3, 0x37, 0x33, 0x65, 0x0e, 0x2f, 0x14, 0x51,
        0xa9, 0xf2, 0x73, 0xe0, 0x13, 0x06, 0x3c, 0xe4 };
        private static string tunnel_server = "tunnel.hologram.io";
        public static PrivateKeyFile[] spacebridge_key { get; }
        private static Dictionary<int, Tuple<ForwardedPortLocal, SshClient>> forwarded_ports;

        public static void createRSAKey(byte[] publicKey, byte[] privateKey)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram");
            Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "/spacebridge.key", privateKey);
            File.WriteAllBytes(path + "/spacebridge.key.pub", publicKey);
            
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

        public static void createDSSKey()
        {
            using (var dsa = new DSACryptoServiceProvider(2048))
            {
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
        }

        public static void beginForwarding(int local_port, int remote_port)
        {
            using (var client = new SshClient(tunnel_server, remote_port, "htunnel", spacebridge_key))
            {
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
                client.Connect();

                var port = new ForwardedPortLocal("localhost", (uint)local_port, tunnel_server, (uint)remote_port);
                client.AddForwardedPort(port);

                port.Exception += delegate (object sender, ExceptionEventArgs e)
                {
                    Console.WriteLine(e.Exception.ToString());
                };
                port.Start();
                forwarded_ports.Add(local_port, new Tuple<ForwardedPortLocal, SshClient>(port, client));
            }
        }

        public static void stopForwarding(int local_port)
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
