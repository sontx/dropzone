using DropZone.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DropZone.Protocol
{
    public class Station
    {
        private const string HeaderPrefix = "DROP_ZONE";

        public string Name { get; }
        private readonly UdpClient _client;
        private readonly ThreadWrapper _thread;
        private readonly Dictionary<string, Neighbor> _neighbors = new Dictionary<string, Neighbor>();
        private readonly Timer _timer;
        private readonly string _id;

        public Action OnNeighborsChanged { get; set; }

        public List<Neighbor> Neighbors
        {
            get
            {
                lock (this)
                {
                    return _neighbors.Values.ToList();
                }
            }
        }

        public Station(string name)
        {
            Name = name;
            _id = Guid.NewGuid().ToString();
            _client = new UdpClient();
            _thread = new ThreadWrapper
            {
                DoWork = ScanNeighbors
            };
            _timer = new Timer(UpdateNeighborsStatus);
        }

        public Neighbor GetNeighbor(string address)
        {
            lock (this)
            {
                var found = _neighbors.Values.FirstOrDefault(neighbor => neighbor.Address == address);
                return found ?? new Neighbor { Address = address };
            }
        }

        private void ScanNeighbors()
        {
            while (true)
            {
                var from = new IPEndPoint(IPAddress.Any, 0);
                var buffer = _client.Receive(ref from);
                var header = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                Debugger.Log("Received broadcast header: " + header);

                var pingInfo = PingInfo.Parse(header);
                if (pingInfo == null || pingInfo.Id == _id)
                    continue;

                var address = from.Address.ToString();
                var neighbor = new Neighbor
                {
                    Name = pingInfo.Name,
                    LastOnline = DateTime.Now,
                    Address = address
                };

                lock (this)
                {
                    var oldSize = _neighbors.Count;

                    _neighbors[address] = neighbor;

                    if (oldSize != _neighbors.Count)
                        OnNeighborsChanged?.Invoke();
                }
            }
        }

        private void UpdateNeighborsStatus(object state)
        {
            Ping();

            lock (this)
            {
                var now = DateTime.Now;
                var pairs = _neighbors.ToArray();
                foreach (var pair in pairs)
                {
                    if (now - pair.Value.LastOnline > Constants.ShortestOnlineTime)
                    {
                        _neighbors.Remove(pair.Key);
                    }
                }

                if (_neighbors.Count != pairs.Length)
                    OnNeighborsChanged?.Invoke();
            }
        }

        private async void Ping()
        {
            var header = new PingInfo { Id = _id, Name = Name }.BuildMessage();
            var bytes = Encoding.UTF8.GetBytes(header);
            await _client.SendAsync(bytes, bytes.Length, "255.255.255.255", Constants.StationPort);
        }

        public void Start()
        {
            _client.Client.Bind(new IPEndPoint(IPAddress.Any, Constants.StationPort));
            _thread.Start();
            _timer.Change(0, 2000);
        }

        public void Stop()
        {
            _timer.Dispose();
            _client.Close();
        }

        public class Neighbor
        {
            public string Address { get; set; }
            public string Name { get; set; }
            public DateTime LastOnline { get; set; }

            public override string ToString()
            {
                var name = string.IsNullOrEmpty(Name) ? "UNKNOWN" : Name;
                return $"{name} [{Address}]";
            }
        }

        private class PingInfo
        {
            public string Name { get; set; }
            public string Id { get; set; }

            public string BuildMessage()
            {
                return $"{HeaderPrefix}|{Name}|{Id}";
            }

            public static PingInfo Parse(string st)
            {
                if (string.IsNullOrEmpty(st))
                    return null;

                var parts = st.Split(new[] { '|' }, 3);
                if (parts.Length != 3 || parts[0] != HeaderPrefix)
                    return null;

                var name = parts[1];
                var id = parts[2];
                return new PingInfo
                {
                    Name = name,
                    Id = id
                };
            }
        }
    }
}