using System.Collections.Generic;
using System.Threading.Tasks;

namespace DropZone
{
    internal class NeighborHelper
    {
        private List<string> _allAddresses = new List<string>();

        public List<string> Neighbors => _allAddresses;

        public NeighborHelper()
        {
            _allAddresses.Add("localhost");
        }

        public Task RefreshAsync()
        {
            return Task.Run(() =>
            {
            });
        }
    }
}