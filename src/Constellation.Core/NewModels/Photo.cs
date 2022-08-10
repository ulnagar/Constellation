namespace Constellation.Core.NewModels;

using Constellation.Core.Common;
using System;

public abstract class Photo : BaseEntity
{
    public byte[] Contents { get; set; }
}

public class StudentPhoto : Photo
{
    public Guid StudentId { get; set; }
    public virtual Student Student { get; set; }
}
