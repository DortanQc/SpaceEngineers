using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private const int ALLOWED_OPEN_SECOND = 5;
        private readonly string[,] _doorGroups =
        {
            { "Station - Door 1 - Airlock 1", "Station - Door 2 - Airlock 1" },
            { "Station - Door 1 - Airlock 2", "Station - Door 2 - Airlock 2" }
        };
        private readonly Dictionary<long, DateTime> _doorOpenedCount = new Dictionary<long, DateTime>();
        private readonly List<AirLockDoors> _doors = new List<AirLockDoors>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            try
            {
                for (var x = 0; x < _doorGroups.GetLength(0); x += 1)
                {
                    var door1 = GridTerminalSystem.GetBlockWithName(_doorGroups[x, 0]) as IMyDoor;
                    var door2 = GridTerminalSystem.GetBlockWithName(_doorGroups[x, 1]) as IMyDoor;

                    if (door1 == null)
                        throw new Exception($"{_doorGroups[x, 0]} block name does noes exists");

                    if (door2 == null)
                        throw new Exception($"{_doorGroups[x, 1]} block name does noes exists");

                    door1.ApplyAction("OnOff_Off");
                    door2.ApplyAction("OnOff_Off");

                    _doors.Add(new AirLockDoors
                    {
                        Door1 = door1,
                        Door2 = door2
                    });
                }
            }
            catch (Exception ex)
            {
                Echo("Error");
                Echo(ex.ToString());
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            SetDoorLockStatus();
            ShutDownDoorWhenOpenedLongerThanExpected();
        }

        private void ShutDownDoorWhenOpenedLongerThanExpected()
        {
            var doors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);

            foreach (var door in doors.OfType<IMyDoor>().Where(d => d.BlockDefinition.TypeIdString != "MyObjectBuilder_AirtightHangarDoor"))
            {
                if (!_doorOpenedCount.ContainsKey(door.EntityId))
                    _doorOpenedCount.Add(door.EntityId, DateTime.Now);

                if (door.Status == DoorStatus.Closed)
                {
                    _doorOpenedCount[door.EntityId] = DateTime.Now;
                    Echo($"{door.CustomName}: Closed");
                }
                else
                {
                    var totalSeconds = DateTime.Now.Subtract(_doorOpenedCount[door.EntityId]).TotalSeconds;

                    Echo($"{door.CustomName}: Opened, {totalSeconds}");

                    if (totalSeconds > ALLOWED_OPEN_SECOND)
                        door.CloseDoor();
                }
            }
        }

        private void SetDoorLockStatus()
        {
            _doors.ForEach(doors =>
            {
                doors.Door1.ApplyAction(doors.Door2.Status != DoorStatus.Closed
                    ? "OnOff_Off"
                    : "OnOff_On");

                doors.Door2.ApplyAction(doors.Door1.Status != DoorStatus.Closed
                    ? "OnOff_Off"
                    : "OnOff_On");
            });
        }

        private class AirLockDoors
        {
            public IMyDoor Door1 { get; set; }

            public IMyDoor Door2 { get; set; }
        }
    }
}
