using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;

namespace PowerLab.Core.Events
{
    /// <summary>
    /// 视图导航请求事件
    /// </summary>
    public class RequestViewEvent : PubSubEvent<string>
    {
    }
}
