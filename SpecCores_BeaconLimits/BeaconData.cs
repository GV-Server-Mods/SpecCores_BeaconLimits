using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using VRage.Game.ModAPI;

namespace BeaconLimits
{
    [ProtoContract]
    public class BeaconList
    {
        [ProtoMember(1)] public List<FactionList> factionList = new List<FactionList>();

        public BeaconList() { }

        public FactionList FindGridById(long entityId)
        {
            foreach (var data in factionList)
            {
                foreach (var dat2 in data.beaconData)
                {
                    if (entityId == dat2.gridId)
                        return data;
                }
            }

            return null;
        }

        /*public FactionList FindDataByBeaconId(long beaconID)
        {
            foreach (var data in factionList)
            {
                foreach (var dat2 in data.beaconData)
                {
                    foreach (var beacon in dat2.beacons)
                    {
                        if (beaconID == beacon)
                            return data;
                    }
                }
            }

            return null;
        }*/

        public FactionList FindDataFromFactionId(long factionID)
        {
            foreach (var data in factionList)
            {
                if (factionID == data.factionId)
                    return data;
            }

            return null;
        }

        public FactionList FindDataFromOwnerId(long ownerID)
        {
            foreach (var data in factionList)
            {
                if (data.factionId == 0 && ownerID == data.ownerId)
                    return data;
            }

            return null;
        }

        /*public void RemoveDataFromList(long beaconID, string beaconSubtype)
        {
            for (int i = factionList.Count - 1; i >= 0; i--)
            {
                for (int j = factionList[i].beaconData.Count - 1; j >= 0; j--)
                {
                    for(int k = factionList[i].beaconData[j].beacons.Count - 1; k >= 0; k--)
                    {
                        if (factionList[i].beaconData[j].beacons[k] == beaconID)
                        {
                            if (factionList[i].beaconData[j].beacons.Count > 1)
                                factionList[i].beaconData[j].beacons.RemoveAt(k);
                            else
                                factionList[i].beaconData.RemoveAt(j);

                            return;
                        }
                    }
                }
            }
        }*/
    }

    [ProtoContract]
    public class FactionList
    {
        [ProtoMember(1)] public List<BeaconData> beaconData;
        [ProtoMember(2)] public long factionId;
        [ProtoMember(3)] public long ownerId;
        [ProtoMember(4)] public int totalBeacons;
        [ProtoMember(5)] public Dictionary<string, List<long>> totalBeaconTypes;

        public FactionList() { }

        public FactionList(long factionID, string name, long gridId, long beaconId, long ownerID, int total, string beaconSubtype)
        {
            factionId = factionID;
            ownerId = ownerID;
            totalBeacons = total;
            BeaconData data = new BeaconData()
            {
                gridName = name,
                gridId = gridId,
                beacons = new Dictionary<string, List<long>>
                {
                    { beaconSubtype, new List<long>() { beaconId } }
                }
            };

            totalBeaconTypes = new Dictionary<string, List<long>>
            {
                { beaconSubtype, new List<long>() { beaconId } }
            };

            beaconData = new List<BeaconData>() { data };
        }

        public void AddBeaconSubtype(string beaconSubtype, long beaconId) 
        {
            if (totalBeaconTypes.ContainsKey(beaconSubtype))
                totalBeaconTypes[beaconSubtype].Add(beaconId);
            else
                totalBeaconTypes.Add(beaconSubtype, new List<long>() { beaconId } );
        }

        public BeaconData GetBeaconDataFromGridId(long gridID)
        {
            foreach(var beacon in beaconData)
            {
                if (beacon.gridId == gridID)
                    return beacon;
            }

            return null;
        }
    }

    [ProtoContract]
    public class BeaconData
    {
        [ProtoMember(1)] public string gridName;
        [ProtoMember(2)] public long gridId;
        [ProtoMember(3)] public Dictionary<string, List<long>> beacons;

        public BeaconData() { }

        public BeaconData(string subtype, long beaconId)
        {
            beacons.Add(subtype, new List<long>() { beaconId });
        }

        public BeaconData(string name, long gridID, long beaconID, string beaconSubtype)
        {
            gridName = name;
            gridId = gridID;
            beacons = new Dictionary<string, List<long>>
            {
                { beaconSubtype, new List<long>() { beaconID } }
            };
        }
    }






    [ProtoContract]
    public class BeaconDataCache
    {
        [ProtoMember(1)] public List<FactionDataCache> dataCache = new List<FactionDataCache>();

        public FactionDataCache GetDataFromFactionId(long factionId)
        {
            foreach (var data in dataCache)
                if (factionId == data.factionId) return data;

            return null;
        }

        public void AddFactionData(long factionId, long beaconId, string beaconType)
        {
            BeaconTypesCache beaconCache = new BeaconTypesCache()
            {
                beaconType = beaconType,
                activeBeaconIds = new List<long> { beaconId }
            };

            FactionDataCache factionCache = new FactionDataCache()
            {
                factionId = factionId,
                playerId = 0,
                beaconTypes = new List<BeaconTypesCache> { beaconCache }
            };
        }

        

        
    }

    [ProtoContract]
    public class FactionDataCache
    {
        [ProtoMember(1)] public long factionId;
        [ProtoMember(2)] public long playerId;
        [ProtoMember(3)] public List<BeaconTypesCache> beaconTypes;

        public BeaconTypesCache GetBeaconCacheByType(string type)
        {
            foreach (var subtypes in beaconTypes)
                if (subtypes.beaconType == type) return subtypes;

            return null;
        }

        public void AddNewBeaconType(string type, long beaconId)
        {
            BeaconTypesCache cache = new BeaconTypesCache()
            {
                beaconType = type,
                beaconLimit = GetBeaconLimit(type),
                activeBeaconIds = new List<long> { beaconId }
            };
        }

        public int GetBeaconLimit(string beaconType)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(factionId);
            int limit = 0;
            foreach (var subtypes in Session.Instance.config._beaconSubtypes)
            {
                if (subtypes.subtype == beaconType)
                {
                    if (faction != null)
                    {
                        if (faction.Members.Count >= subtypes.limit.Length)
                            return limit = subtypes.limit.Last();
                        else
                            return limit = subtypes.limit[faction.Members.Count - 1];
                    }
                    else
                        return limit = subtypes.limit.First();
                }
            }

            return limit;
        }
    }

    [ProtoContract]
    public class BeaconTypesCache
    {
        [ProtoMember(1)] public string beaconType;
        [ProtoMember(2)] public int beaconLimit;
        [ProtoMember(3)] public List<long> activeBeaconIds;
        [ProtoMember(4)] public List<long> overLimitBeaconIds;

        public bool WillAddingBeaconGoOverLimit(long factionId)
        {
            return activeBeaconIds.Count >= beaconLimit;
        }

        public void AddBeacon(long beaconId)
        {
            if (!activeBeaconIds.Contains(beaconId))
                activeBeaconIds.Add(beaconId);
        }

        public void AddBeaconOverLimit(long beaconId)
        {
            if (!overLimitBeaconIds.Contains(beaconId))
                overLimitBeaconIds.Add(beaconId);
        }

        public void RemoveBeacon(long beaconId)
        {
            if (activeBeaconIds.Contains(beaconId))
                activeBeaconIds.Remove(beaconId);
        }

        public void MigrateOverLimitsToActive(long beaconId)
        {
            if (!activeBeaconIds.Contains(beaconId))
                activeBeaconIds.Add(beaconId);

            if (overLimitBeaconIds.Contains(beaconId))
                overLimitBeaconIds.Remove(beaconId);
        }

        public void RemoveBeaconOverLimit(long beaconId)
        {
            if (overLimitBeaconIds.Contains(beaconId))
                overLimitBeaconIds.Remove(beaconId);
        }
    }
}