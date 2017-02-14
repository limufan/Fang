using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fang
{
    public delegate void TEventHandler<SenderType, ArgsType>(SenderType sender, ArgsType args);
    public delegate void TEventHandler<Args>(Args args);
}
