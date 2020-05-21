using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
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
        private static SshClient client = null;

        public static void SaveRSAKey(byte[] publicKey, byte[] privateKey)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram");
            Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "/spacebridge.key", privateKey);
            File.WriteAllBytes(path + "/spacebridge.key.pub", publicKey);

            spacebridge_key.Add(new PrivateKeyFile(path + "/spacebridge.key"));
        }

        public static void GenerateRSAKey()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram");
                    Directory.CreateDirectory(path);
                    File.WriteAllBytes(path + "/spacebridge.key", rsa.ExportRSAPrivateKey());
                    File.WriteAllBytes(path + "/spacebridge.key.pub", rsa.ExportRSAPublicKey());
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static void GenerateDSSKey()
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
            if (client != null && client.ForwardedPorts.Select(port => ((ForwardedPortLocal)port).BoundPort).ToList().Contains((uint)local_port))
            {
                MessageBox.Show("Local port is already forwarding, use a different port", "SSH Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (spacebridge_key.Count == 0)
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hologram");
                spacebridge_key.Add(new PrivateKeyFile(path + "/spacebridge.key"));
            }
            if (client == null)
            {
                client = new SshClient(tunnel_server, tunnel_port, "htunnel", spacebridge_key.ToArray())
                {
                    KeepAliveInterval = new TimeSpan(0, 0, 30)
                };
                client.HostKeyReceived += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("Got finerprint");
                    if (hologram_fingerprint.Length == e.FingerPrint.Length)
                    {
                        for (var i = 0; i < hologram_fingerprint.Length; i++)
                        {
                            if (hologram_fingerprint[i] != e.FingerPrint[i])
                            {
                                System.Diagnostics.Debug.WriteLine("Finerprint Denied");
                                e.CanTrust = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Finerprint Denied");
                        e.CanTrust = false;
                    }
                };
                try
                {
                    client.Connect();
                    System.Diagnostics.Debug.WriteLine("Client Connected");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "SSH Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return false;
                }
                client.ErrorOccurred += Client_ErrorOccurred;
            }

            var port = new ForwardedPortLocal("localhost", (uint)local_port, "link" + linkId, (uint)remote_port);
            client.AddForwardedPort(port);

            port.Exception += delegate (object sender, ExceptionEventArgs e)
            {
                MessageBox.Show("Spacebridge encountered an error:\n" + e.Exception.Message, "SSH Error", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
            };

            port.RequestReceived += delegate (object sender, PortForwardEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            };
            port.Start();
            System.Diagnostics.Debug.WriteLine("Forwarding " + port.BoundHost + ":" + port.BoundPort + " to " + port.Host + ":" + port.Port);
            return true;
        }

        private static void Client_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "SSH Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
        }

        public static void StopForwarding(int local_port)
        {
            foreach (ForwardedPortLocal port in client.ForwardedPorts)
            {
                if (port.BoundPort == local_port)
                {
                    port.Stop();
                }
            }
        }
    }
}
