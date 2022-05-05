using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript.Scripts.AirLock
{
    public class Program : MyGridProgram
    {
        private const string DOOR1 = "Station - Door01";
        private const string DOOR2 = "Station - Door02";
        private const int ALLOWED_OPEN_SECOND = 5;
        private readonly IMyDoor _door1;
        private readonly IMyDoor _door2;
        private readonly Dictionary<long, DateTime> _doorOpenedCount = new Dictionary<long, DateTime>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            try
            {
                _door1 = GridTerminalSystem.GetBlockWithName(DOOR1) as IMyDoor;
                _door2 = GridTerminalSystem.GetBlockWithName(DOOR2) as IMyDoor;

                if (_door1 == null)
                    throw new Exception($"{DOOR1} block name does noes exists");

                if (_door2 == null)
                    throw new Exception($"{DOOR2} block name does noes exists");

                _door1.ApplyAction("OnOff_Off");
                _door2.ApplyAction("OnOff_Off");
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
            _door1.ApplyAction(_door2.Status != DoorStatus.Closed
                ? "OnOff_Off"
                : "OnOff_On");

            _door2.ApplyAction(_door1.Status != DoorStatus.Closed
                ? "OnOff_Off"
                : "OnOff_On");

            UpdateDoorStatus(_door1);
            UpdateDoorStatus(_door2);
        }

        private void UpdateDoorStatus(IMyDoor door)
        {
            if (!_doorOpenedCount.ContainsKey(door.EntityId))
                _doorOpenedCount.Add(door.EntityId, DateTime.Now);

            if (door.Status == DoorStatus.Closed)
            {
                _doorOpenedCount[door.EntityId] = DateTime.Now;
            }
            else
            {
                if (DateTime.Now.Subtract(_doorOpenedCount[door.EntityId]).TotalSeconds > ALLOWED_OPEN_SECOND)
                    door.CloseDoor();
            }
        }
    }
}