using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CleverChair.model
{
    [DataContract]
    class FirebaseResponseData
    {
        [DataMember(Name ="soundMuteTimeout")]
        public long SoundMuteTimeout { get; set; }

        [DataMember(Name = "micMuteTimeout")]
        public long MicMuteTimeout { get; set; }

        [DataMember(Name = "lockTimeout")]
        public long LockTimeout { get; set; }

        [DataMember(Name = "freeTime")]
        public long FreeTime { get; set; }

        [DataMember(Name = "busyTime")]
        public long BusyTime { get; set; }
    }
}
