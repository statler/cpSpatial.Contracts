using System;
using System.Linq;

public enum SpatialElementInteractionActionEnum
{
    None = 0,

    Terminate = 1,
    TrimToBoundary = 2,

    SubtractVolume = 3,
    AddOpening = 4,

    OffsetAround = 5,
    StepOver = 6,

    Split = 7,
    Merge = 8,

    ClearanceOnly = 9,
    ClearanceEnvelope = 10,

    Ignore = 11
}
