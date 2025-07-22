using ReactiveUI;
using System.ComponentModel;

namespace KakeboApp.Test
{
    public class TestViewModel : ReactiveObject
    {
        public string TestProperty { get; set; } = string.Empty;
    }
}
