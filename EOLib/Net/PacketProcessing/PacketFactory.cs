using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Optional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EOLib.Net.PacketProcessing
{
    public class PacketFactory : IPacketFactory
    {
        private readonly IReadOnlyDictionary<FamilyActionPair, Type> _map;

        public PacketFactory(string name_space)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetTypes().Select(x => x.Namespace).Any(x => x == name_space))
                {
                    _map = MapTypesFrom(assembly, name_space);
                    break;
                }
            }

            if (!_map.Any())
            {
                throw new ArgumentException($"No packets were found from {name_space}", nameof(name_space));
            }
        }

        public Option<IPacket> Create(byte[] array)
        {
            var fap = FamilyActionPair.From(array);
            if (!_map.ContainsKey(fap))
            {
                Debug.WriteLine($"Unrecognized packet id: {fap.Family}_{fap.Action}");
                return Option.None<IPacket>();
            }

            var instance = (IPacket)Activator.CreateInstance(_map[fap]);
            var eoReader = new EoReader(array);
            instance.Deserialize(eoReader.Slice(2));
            return Option.Some(instance);
        }

        private static IReadOnlyDictionary<FamilyActionPair, Type> MapTypesFrom(Assembly assembly, string name_space)
        {
            var ret = new Dictionary<FamilyActionPair, Type>();

            var types = assembly.GetTypes()
                .Where(x => x.Namespace == name_space)
                .Where(x => x.GetInterfaces().Any(x => x.Name == "IPacket"));
            foreach (var type in types)
            {
                var inst = (IPacket)Activator.CreateInstance(type);
                var fap = new FamilyActionPair(inst.Family, inst.Action);
                ret.Add(fap, type);
            }

            return ret;
        }
    }

    public interface IPacketFactory
    {
        Option<IPacket> Create(byte[] array);
    }
}