// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

namespace Arc.WinUI;

/// <summary>
/// Counter with garbage collection count difference check. If the counter reaches a certain threshold, it checks the garbage collection count and returns true if the count has changed.
/// </summary>
internal class GCCountChecker
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GCCountChecker"/> class.
    /// </summary>
    /// <param name="maxCount">The maximum count before checking the garbage collection count.</param>
    public GCCountChecker(int maxCount = 0)
    {
        this.Count = 0;
        this.MaxCount = maxCount;
        this.peviousGCCount = 0;
    }

    /// <summary>
    /// Gets the current count.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the maximum count before checking the garbage collection count.
    /// </summary>
    public int MaxCount { get; }

    private int peviousGCCount;

    /// <summary>
    /// If the counter exceeds a certain level, check the garbage collection counter, and if the counter has changed, return true.
    /// </summary>
    /// <returns>True if the garbage collection count has changed; otherwise, false.</returns>
    public bool Check()
    {
        this.Count++;
        if (this.Count >= this.MaxCount)
        {
            this.Count = 0;
            var count = GC.CollectionCount(0);
            if (count != this.peviousGCCount)
            {
                this.peviousGCCount = count;
                return true;
            }
        }

        return false;
    }
}
