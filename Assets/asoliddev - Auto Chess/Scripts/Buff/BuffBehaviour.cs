using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffBehaviour
{
    public BuffController _controller;
    public virtual void BuffAwake() { }
    public virtual void BuffStart() { }
    public virtual void BuffRefresh() { }
    public virtual void BuffRemove() { }
    public virtual void BuffDestroy() { }
    public virtual void BuffInterval() { }
    public virtual void BuffActive() { }
}
