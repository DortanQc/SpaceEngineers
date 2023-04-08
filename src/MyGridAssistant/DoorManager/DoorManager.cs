using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGridAssistant
{
    public class DoorManager
    {
        private const int ALLOWED_OPEN_SECOND = 5;
        private static readonly Dictionary<long, DateTime> DoorOpenedCount = new Dictionary<long, DateTime>();

        public static void ShutDownDoorWhenOpenedLongerThanExpected(List<IMyDoor> doors)
        {
            foreach (var door in doors)
            {
                if (!DoorOpenedCount.ContainsKey(door.EntityId))
                    DoorOpenedCount.Add(door.EntityId, DateTime.Now);

                if (door.Status == DoorStatus.Closed)
                {
                    DoorOpenedCount[door.EntityId] = DateTime.Now;
                }
                else
                {
                    var totalSeconds = DateTime.Now.Subtract(DoorOpenedCount[door.EntityId]).TotalSeconds;

                    if (totalSeconds > ALLOWED_OPEN_SECOND)
                        door.CloseDoor();
                }
            }
        }

        public static void ManageAirLocks(Action<string> echo, List<IMyDoor> doors)
        {
            var airlockDictionary = new Dictionary<string, List<IMyDoor>>();

            doors.ForEach(door =>
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
