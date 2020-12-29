using DropZone.Protocol;
using DropZone.Utils;
using System;
using System.Collections.ObjectModel;

namespace DropZone.ViewModels
{
    public partial class MainViewModel
    {
        private Station _station;
        private string _neighborsSummary;
        private bool _hasNeighbors;

        public string NeighborsSummary
        {
            get => _neighborsSummary;
            private set => Set(ref _neighborsSummary, value);
        }

        public bool HasNeighbors
        {
            get => _hasNeighbors;
            private set => Set(ref _hasNeighbors, value);
        }

        public ObservableCollection<NeighborMenuItemViewModel> NeighborMenuItems { get; } =
            new ObservableCollection<NeighborMenuItemViewModel>();

        private void InitializeStation()
        {
            _station = new Station(Environment.MachineName);
            _station.Start();
            _station.OnNeighborsChanged = UpdateNeighborsSummary;
            UpdateNeighborsSummary();
        }

        private void UpdateNeighborsSummary()
        {
            Debugger.Log("Online neighbors are updated");

            ThreadUtils.RunOnUi(() =>
            {
                var neighbors = _station.Neighbors;

                if (neighbors.Count == 1)
                {
                    NeighborsSummary = $"{neighbors[0].Name} is online";
                }
                else if (neighbors.Count > 1)
                {
                    NeighborsSummary = $"{neighbors.Count} neighbors are online";
                }

                HasNeighbors = neighbors.Count > 0;
                IsReadyToSend = HasNeighbors;

                NeighborMenuItems.Clear();
                foreach (var neighbor in neighbors)
                {
                    NeighborMenuItems.Add(new NeighborMenuItemViewModel(this, neighbor));
                }
            });
        }

        private void CloseStation()
        {
            _station.Stop();
        }
    }
}