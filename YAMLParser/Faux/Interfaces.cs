﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using String = FauxMessages.std_msgs.String;
using uint8 = System.Byte;

namespace FauxMessages
{
    public class IRosMessage
    {
        internal static Dictionary<MsgTypes, Func<MsgTypes, IRosMessage>> constructors = new Dictionary<MsgTypes, Func<MsgTypes, IRosMessage>>();
        private static Dictionary<MsgTypes, Type> _typeregistry = new Dictionary<MsgTypes, Type>();
        [DebuggerStepThrough]
        public static IRosMessage generate(MsgTypes t)
        {
            if (constructors.ContainsKey(t))
                return constructors[t].Invoke(t);
            Type thistype = typeof(IRosMessage);
            foreach (Type othertype in thistype.Assembly.GetTypes())
            {
                if (thistype == othertype || !othertype.IsSubclassOf(thistype)) continue;
                IRosMessage msg = Activator.CreateInstance(othertype) as IRosMessage;
                if (msg != null)
                {
                    if (msg.msgtype == MsgTypes.Unknown)
                        throw new Exception("OH NOES IRosMessage.generate is borked!");
                    if (!_typeregistry.ContainsKey(msg.msgtype))
                        _typeregistry.Add(msg.msgtype, msg.GetType());
                    if (!constructors.ContainsKey(msg.msgtype))
                        constructors.Add(msg.msgtype, T => Activator.CreateInstance(_typeregistry[T]) as IRosMessage);
                }
            }
            if (constructors.ContainsKey(t))
                return constructors[t].Invoke(t);
            else
                throw new Exception("OH NOES IRosMessage.generate is borked!");
        }

        public string MD5Sum;
        public bool HasHeader;
        public bool IsMetaType;
        public string MessageDefinition;
        public byte[] Serialized;
        public IDictionary connection_header;
        public MsgTypes msgtype;
        public bool IsServiceComponent;
        public Dictionary<string, MsgFieldInfo> Fields;

        public IRosMessage()
            : this(MsgTypes.Unknown, "", false, false, null)
        {
        }

        [DebuggerStepThrough]
        public IRosMessage(MsgTypes t, string def, bool hasheader, bool meta, Dictionary<string, MsgFieldInfo> fields)
        :this(t,def,hasheader,meta,fields,"")
        {}

        [DebuggerStepThrough]
        public IRosMessage(MsgTypes t, string def, bool hasheader, bool meta, Dictionary<string, MsgFieldInfo> fields, string ms5) : this(t,def,hasheader,meta,fields,ms5,false)
        {
        }

        [DebuggerStepThrough]
        public IRosMessage(MsgTypes t, string def, bool hasheader, bool meta, Dictionary<string, MsgFieldInfo> fields, string ms5, bool isservicemessage)
        {
            msgtype = t;
            MessageDefinition = def;
            HasHeader = hasheader;
            IsMetaType = meta;
            Fields = fields;
            MD5Sum = ms5;
            IsServiceComponent = isservicemessage;
        }

        public IRosMessage(byte[] SERIALIZEDSTUFF)
        {
            int dontcare = 0;
            SerializationHelper.deserialize(GetType(), null, SERIALIZEDSTUFF, out dontcare, !IsMetaType && msgtype != MsgTypes.std_msgs__String);
        }

        //[DebuggerStepThrough]
        public virtual IRosMessage Deserialize(byte[] SERIALIZEDSTUFF)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] Serialize()
        {
            return Serialize(false);
        }

        public virtual byte[] Serialize(bool partofsomethingelse)
        {
            throw new NotImplementedException();
        }
    }

    public delegate IRosMessage RosServiceDelegate(IRosMessage request);

    public class IRosService
    {
        public string MD5Sum;
        public string ServiceDefinition;
        public SrvTypes srvtype = SrvTypes.Unknown;
        public IRosMessage RequestMessage, ResponseMessage;

        protected IRosMessage GeneralInvoke(RosServiceDelegate invocation, IRosMessage m)
        {
            return invocation.Invoke(m);
        }
        public IRosService()
            : this(SrvTypes.Unknown, "", "")
        {
        }

        public IRosService(SrvTypes t, string def, string md5)
        {
            srvtype = t;
            ServiceDefinition = def;
            MD5Sum = md5;
        }

        protected void InitSubtypes(IRosMessage request, IRosMessage response)
        {
            RequestMessage = request;
            ResponseMessage = response;
        }

        internal static Dictionary<SrvTypes, Func<SrvTypes, IRosService>> constructors = new Dictionary<SrvTypes, Func<SrvTypes, IRosService>>();
        private static Dictionary<SrvTypes, Type> _typeregistry = new Dictionary<SrvTypes, Type>();

        [DebuggerStepThrough]
        public static IRosService generate(SrvTypes t)
        {
            if (constructors.ContainsKey(t))
                return constructors[t].Invoke(t);
            Type thistype = typeof(IRosService);
            foreach (Type othertype in thistype.Assembly.GetTypes())
            {
                if (thistype == othertype || !othertype.IsSubclassOf(thistype)) continue;
                IRosService srv = Activator.CreateInstance(othertype) as IRosService;
                if (srv != null)
                {
                    if (srv.srvtype == SrvTypes.Unknown)
                        throw new Exception("OH NOES IRosMessage.generate is borked!");
                    if (!_typeregistry.ContainsKey(srv.srvtype))
                        _typeregistry.Add(srv.srvtype, srv.GetType());
                    if (!constructors.ContainsKey(srv.srvtype))
                        constructors.Add(srv.srvtype, T => Activator.CreateInstance(_typeregistry[T]) as IRosService);
                }
            }
            if (constructors.ContainsKey(t))
                return constructors[t].Invoke(t);
            else
                throw new Exception("OH NOES IRosMessage.generate is borked!");
        }
    }

    public class MsgFieldInfo
    {
        public string ConstVal;
        public bool IsArray;
        public bool IsConst;
        public bool IsLiteral;
        public bool IsMetaType;
        public int Length = -1;
        public string Name;
        public Type Type;
        [DebuggerStepThrough]
        public MsgFieldInfo(string name, bool isliteral, Type type, bool isconst, string constval, bool isarray,
                            string lengths, bool meta)
        {
            Name = name;
            IsArray = isarray;
            Type = type;
            IsLiteral = isliteral;
            IsMetaType = meta;
            IsConst = isconst;
            ConstVal = constval;
            if (lengths == null) return;
            if (lengths.Length > 0)
            {
                Length = int.Parse(lengths);
            }
        }
    }



    public enum ServiceMessageType
    {
        Not,
        Request,
        Response
    }

    public struct TimeData
    {
        public uint sec;
        public uint nsec;

        public TimeData(uint s, uint ns)
        {
            sec = s;
            nsec = ns;
        }
    }
}
