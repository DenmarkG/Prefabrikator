using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prefabrikator.Editor
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
    }
}
