﻿using System.Runtime.Serialization;

namespace cc.newspring
{
    [DataContract]
    public class Model<T> : Rock.Data.Model<T> where T : Rock.Data.Model<T>, Rock.Security.ISecured, new()
    {
    }
}
