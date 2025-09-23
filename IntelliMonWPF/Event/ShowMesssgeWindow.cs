using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events; 


namespace IntelliMonWPF.Event
{
    // 修正：PubSubEvent 只支持一个泛型参数
    public class ShowMesssgeWindow : PubSubEvent<Tuple<string, int>>
    {

    }
}
