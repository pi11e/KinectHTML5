using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using Microsoft.Kinect;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace Kinect.Server
{
    /// <summary>
    /// Serializes a Kinect skeleton to JSON fromat.
    /// </summary>
    public static class SkeletonSerializer
    {
        [DataContract]
        class JSONSkeletonCollection
        {
            [DataMember(Name = "skeletons")]
            public List<JSONSkeleton> Skeletons { get; set; }

            [DataMember(Name = "type")]
            public string type { get; set; }
        }

        [DataContract]
        class JSONString
        {
            [DataMember(Name = "message")]
            public string message { get; set; }

            [DataMember(Name = "type")]
            public string type { get; set; }
        }

        [DataContract]
        class JSONSkeleton
        {
            [DataMember(Name = "id")]
            public string ID { get; set; }

            [DataMember(Name = "joints")]
            public List<JSONJoint> Joints { get; set; }
        }

        [DataContract]
        class JSONJoint
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "x")]
            public double X { get; set; }

            [DataMember(Name = "y")]
            public double Y { get; set; }

            [DataMember(Name = "z")]
            public double Z { get; set; }
        }

        public static string Serialize(this List<Skeleton> skeletons)
        {
            JSONSkeletonCollection jsonSkeletons = new JSONSkeletonCollection { Skeletons = new List<JSONSkeleton>() };

            foreach (var skeleton in skeletons)
            {
                JSONSkeleton jsonSkeleton = new JSONSkeleton
                {
                    ID = skeleton.TrackingId.ToString(),
                    Joints = new List<JSONJoint>()
                };

                foreach (Joint joint in skeleton.Joints)
                {
                    Joint scaled = joint.ScaleTo(640, 480);

                    jsonSkeleton.Joints.Add(new JSONJoint
                    {
                        Name = scaled.JointType.ToString().ToLower(),
                        X = scaled.Position.X,
                        Y = scaled.Position.Y,
                        Z = scaled.Position.Z
                    });
                }

                jsonSkeletons.Skeletons.Add(jsonSkeleton);
                jsonSkeletons.type = "JSONSkeletonCollection";
            }

            return Serialize(jsonSkeletons);
        }

        public static string toJSON(object obj)
        {
            if(obj is string)
            {
                JSONString jstring = new JSONString();
                jstring.type = "JSONstring";
                jstring.message = (string)obj;
            }

            return Serialize(obj);
        }

        // Resource: http://pietschsoft.com/post/2008/02/NET-35-JSON-Serialization-using-the-DataContractJsonSerializer.aspx.
        private static string Serialize(object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            string retVal = Encoding.Default.GetString(ms.ToArray());
            ms.Dispose();

            return retVal;
        }
    }
}
