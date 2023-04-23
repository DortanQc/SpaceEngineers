using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGridAssistant
{
    public class DoorManager
    {
        private const int ALLOWED_OPEN_SECOND = 5;
        private readonly Dictionary<long, DateTime> _doorOpenedCount = new Dictionary<long, DateTime>();
        private readonly IMyGridAssistantLogger _logger;

        public DoorManager(IMyGridAssistantLogger logger)
        {
            _logger = logger;
        }

        public void ShutDownDoorWhenOpenedLongerThanExpected(IEnumerable<BlockEntity<IMyDoor>> doors)
        {
            var doorsToScan = doors
                .Where(door => door.Exists())
                .Select(door => door.Block)
                .ToList();

            foreach (var door in doorsToScan)
            {
                if (!_doorOpenedCount.ContainsKey(door.EntityId))
                    _doorOpenedCount.Add(door.EntityId, DateTime.Now);

                if (door.Status == DoorStatus.Closed)
                {
                    _doorOpenedCount[door.EntityId] = DateTime.Now;
                }
                else
                {
                    var totalSeconds = DateTime.Now.Subtract(_doorOpenedCount[door.EntityId]).TotalSeconds;

                    if (totalSeconds > ALLOWED_OPEN_SECOND)
                        door.CloseDoor();
                }
            }
        }

        public void ManageAirLocks(IEnumerable<BlockEntity<IMyDoor>> doors)
        {
            var doorsToScan = doors
                .Where(door => door.Exists())
                .Select(door => door.Block)
                .ToList();

            var airlockDictionary = new Dictionary<string, List<IMyDoor>>();

            doorsToScan.ForEach(door =>
            {
                var group = Configuration.GetBlockConfiguration(door, Settings.IS_AIR_LOCK);

                if (group == null) return;

                var airlocks = new List<IMyDoor>();

                if (airlockDictionary.ContainsKey(group))
                    airlocks = airlockDictionary[group];
                else
                    airlockDictionary.Add(group, new List<IMyDoor>());

                if (airlocks.Count > 2) return;

                airlocks.Add(door);

                airlockDictionary[group] = airlocks;
            });

            airlockDictionary
                .Where(airlock => airlock.Value.Count == 2)
                .ToList()
                .ForEach(d =>
                {
                    var door1 = d.Value[0];
                    var door2 = d.Value[1];

                    door1.ApplyAction(door2.Status != DoorStatus.Closed
                        ? "OnOff_Off"
                        : "OnOff_On");

                    door2.ApplyAction(door1.Status != DoorStatus.Closed
                        ? "OnOff_Off"
                        : "OnOff_On");
                });
        }
    }
}
